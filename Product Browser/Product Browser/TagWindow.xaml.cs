using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel;
using DatabaseModel.Model;
using System.Data.Entity;
using System.Linq;
using Product_Browser.ScatterItems;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Surface.Core;
using System.Windows.Data;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : TagVisualization, INotifyPropertyChanged
    {

        #region Physics and position constants

        // Radius of gravity circle around tag visualization
        readonly double
            CIRCLE_SIZE = 150d;

        readonly int MAX_IMAGES_BEFORE_CONTAINER = 4;

        readonly Size
            SCATTERITEM_DOCUMENT_STARTING_SIZE = new Size(150, 200),
            SCATTERITEM_VIDEO_STARTING_SIZE = new Size(200, 125),
            SCATTERITEM_IMAGE_STARTING_SIZE = new Size(200, 125);

        readonly Vector
            SCATTERITEM_DOCUMENT_STARTING_POSITION = new Vector(0, -180),
            SCATTERITEM_VIDEO_STARTING_POSITION = new Vector(-225, 0),
            SCATTERITEM_IMAGE_STARTING_POSITION = new Vector(225, 0),

            SCATTERITEM_DOCUMENT_POSITION_OFFSET = new Vector(0, -20),
            SCATTERITEM_VIDEO_POSITION_OFFSET = new Vector(-20, 0),
            SCATTERITEM_IMAGE_POSITION_OFFSET = new Vector(20, 0);

        readonly double
            SCATTERITEM_DOCUMENT_STARTING_ROTATION = 0d,
            SCATTERITEM_VIDEO_STARTING_ROTATION = 0d,
            SCATTERITEM_IMAGE_STARTING_ROTATION = 0d;

        

        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Fields

        SmartCard smartCard = null;

        List<ScatterItemPhysics> physicsItemsActive = new List<ScatterItemPhysics>(10);
        List<ScatterItemPhysics> physicsItemsActiveLowPriority = new List<ScatterItemPhysics>(10);
        List<ScatterItemPhysics> physicsItemsInactive = new List<ScatterItemPhysics>(10);

        DispatcherTimer physicsTimer,
                        physicsTimerLowPriority,
                        animationPulseTimer;

        bool pulseUp = false;

        /// <summary>
        /// These are helpers to determine visibility of UI elements in TagWindow.xaml. Start by displaying loading
        /// </summary>
        bool foundSmartCard = false,
            notFoundSmartCard = false,
            loadingSmartCard = true;

        #endregion

        #region Properties

        /// <summary>
        /// These are states, and should only ever be set true
        /// </summary>
        public bool FoundSmartCard { get { return foundSmartCard; }
            set
            {
                if (!value)
                    return;

                foundSmartCard = value;
                loadingSmartCard = !value;
                foundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("LoadingSmartCard");
            }
        }

        public bool NotFoundSmartCard { get { return notFoundSmartCard; }
            set {
                if (!value)
                    return;

                notFoundSmartCard = value;
                foundSmartCard = !value;
                loadingSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("LoadingSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        public bool LoadingSmartCard { get { return loadingSmartCard; }
            set
            {
                if (!value)
                    return;

                loadingSmartCard = value;
                notFoundSmartCard = !value;
                foundSmartCard = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotFoundSmartCard");
                NotifyPropertyChanged("FoundSmartCard");
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnMoved(RoutedEventArgs args)
        {
            base.OnMoved(args);

            // Add all inactive to active
            physicsItemsActive.AddRange(physicsItemsInactive);

            // Clear inactive
            physicsItemsInactive.Clear();

            // Calculate new positions for all of them
            CalculateNewPositions(physicsItemsActive);

            // Start timer if not running
            if (!physicsTimer.IsEnabled)
                physicsTimer.Start();
        }

        private void ScatterViewItemMovedHandler(object sender, EventArgs args)
        {
            if (!physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Start();

            ScatterItemPhysics phy = null;
            foreach(ScatterItemPhysics p in physicsItemsInactive)
            {
                if (p.Item == sender as ScatterViewItem)
                {
                    phy = p;
                    p.Item.Deceleration = 0.001536d;
                    physicsItemsActiveLowPriority.Add(p);
                    break;
                }
            }

            if (phy != null) // If it's null, it already exists in the active container so no need to do anything
            {
                physicsItemsInactive.Remove(phy);

                // Add all inactive to active
                physicsItemsActive.AddRange(physicsItemsInactive);
                
                // Clear inactive
                physicsItemsInactive.Clear();

                // Calculate new positions for all of them
                CalculateNewPositions(physicsItemsActive);

                // Start timer if not running
                if (!physicsTimer.IsEnabled)
                    physicsTimer.Start();
            }
        }

        private void ScatterViewItemLockedHandler(ScatterItemPhysics e)
        {
            physicsItemsInactive.Add(e);

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void ScatterViewItemLowPriorityHandler(ScatterItemPhysics e)
        {
            if (!physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Start();

            e.Item.Deceleration = 0.001536d;

            physicsItemsActiveLowPriority.Add(e);
        }

        private void ScatterViewItemHighPriorityHandler(ScatterItemPhysics e)
        {
            if (!physicsTimer.IsEnabled)
                physicsTimer.Start();
            
            e.Item.Deceleration = double.NaN;

            // Add all inactive to active
            physicsItemsActive.AddRange(physicsItemsInactive);

            // And add this one
            physicsItemsActive.Add(e);
            // Then clear inactive
            physicsItemsInactive.Clear();

            // Calculate new positions for all active items
            CalculateNewPositions(physicsItemsActive);
        }

        private void PhysicsEventHandler(object sender, EventArgs args)
        {
            List<ScatterItemPhysics> toDiscard = new List<ScatterItemPhysics>(physicsItemsActive.Count);
            
            for (int i = 0; i < physicsItemsActive.Count; i++)
                if (physicsItemsActive[i].Run(Center, Orientation))
                    toDiscard.Add(physicsItemsActive[i]);

            physicsItemsActive.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void PhysicsLowPriorityEventHandler(object sender, EventArgs args)
        {
            List<ScatterItemPhysics> toDiscard = new List<ScatterItemPhysics>(physicsItemsActive.Count);

            for (int i = 0; i < physicsItemsActiveLowPriority.Count; i++)
                if (physicsItemsActiveLowPriority[i].RunLowPriority(Center, Orientation, CIRCLE_SIZE))
                    toDiscard.Add(physicsItemsActiveLowPriority[i]);

            physicsItemsActiveLowPriority.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActiveLowPriority.Count == 0)
                physicsTimerLowPriority.Stop();
        }

        private void AnimationPulseHandler(object sender, EventArgs args)
        {
            if (pulseUp)
            {
                animationPulse.Opacity += 0.005;

                if (animationPulse.Opacity >= 0.6d)
                    pulseUp = false;
            }
            else
            {
                animationPulse.Opacity -= 0.005;

                if (animationPulse.Opacity <= 0.15d)
                    pulseUp = true;
            }
            
        }

        #endregion

        #region Methods
        public void DestroySmartCard(ScatterView view)
        {
            for (int i = 0; i < physicsItemsInactive.Count; i++)
                view.Items.Remove(physicsItemsInactive[i].Item);
            for (int i = 0; i < physicsItemsActive.Count; i++)
                view.Items.Remove(physicsItemsActive[i].Item);
            for (int i = 0; i < physicsItemsActiveLowPriority.Count; i++)
                view.Items.Remove(physicsItemsActiveLowPriority[i].Item);

            physicsItemsActive.Clear();
            physicsItemsInactive.Clear();
            physicsItemsActiveLowPriority.Clear();
        }

        private void CalculateNewPositions(List<ScatterItemPhysics> items)
        {
            var documents = items.Where(a => a.Item is DocumentScatterItem).ToList();
            var images = items.Where(a => a.Item is ImageScatterItem || a.Item is ImageContainerScatterItem).ToList();
            var videos = items.Where(a => a.Item is VideoScatterItem).ToList();

            for (int i = 0; i < documents.Count; i++)
            {
                documents[i].OriginalPositionOffset = (Point)(SCATTERITEM_DOCUMENT_STARTING_POSITION + SCATTERITEM_DOCUMENT_POSITION_OFFSET * i);
                documents[i].Item.ZIndex = i;
            }

            for (int i = 0; i < images.Count; i++)
            {
                images[i].OriginalPositionOffset = (Point)(SCATTERITEM_IMAGE_STARTING_POSITION + SCATTERITEM_IMAGE_POSITION_OFFSET * i);
                images[i].Item.ZIndex = i;
            }

            for (int i = 0; i < videos.Count; i++)
            {
                videos[i].OriginalPositionOffset = (Point)(SCATTERITEM_VIDEO_STARTING_POSITION + SCATTERITEM_VIDEO_POSITION_OFFSET * i);
                videos[i].Item.ZIndex = videos.Count - 1 - i;
            }
        }

        public async void InitializeSmartCard(ScatterView view)
        {
            ABBDataContext context = new ABBDataContext();
            
            smartCard = await context.SmartCards
                .Include(s => s.DataItems.Select(d => d.DataField))
                .FirstOrDefaultAsync(a => a.TagId == VisualizedTag.Value);
            
            List<SmartCardDataItem> dataItems = null;

            if (smartCard == null || (dataItems = smartCard.DataItems.ToList()) == null || dataItems.Count == 0)
            {
                NotFoundSmartCard = true;
                return;
            }

            FoundSmartCard = true;

            List<ScatterViewItem> scatterItems = new List<ScatterViewItem>();

            // Place all images inside one scatter item if > max images, else individual scatter items
            dataItems = smartCard.DataItems.Where(a => a.Category == SmartCardDataItemCategory.Image).ToList();
            if(dataItems.Count > MAX_IMAGES_BEFORE_CONTAINER)
            {
                ScatterViewItem item = new ImageContainerScatterItem(dataItems);
                scatterItems.Add(item);
            }
            else
            {
                foreach(SmartCardDataItem dataItem in dataItems)
                {
                    ScatterViewItem item = new ImageScatterItem(dataItem);
                    scatterItems.Add(item);
                }
            }

            // One scatter item each for everything else
            dataItems = smartCard.DataItems.Where(a => a.Category != SmartCardDataItemCategory.Image).ToList();
            for (int i = 0; i < dataItems.Count; i++)
            {
                ScatterViewItem item = null;
                switch (dataItems[i].Category)
                {
                    case SmartCardDataItemCategory.Document:
                        item = new DocumentScatterItem(dataItems[i]);
                        break;
                    case SmartCardDataItemCategory.Video:
                        item = new VideoScatterItem(dataItems[i]);
                        break;
                }
                scatterItems.Add(item);
            }

            for (int i = 0; i < scatterItems.Count; i++)
            {
                ScatterItemPhysics physics = new ScatterItemPhysics(scatterItems[i]);

                scatterItems[i].PreviewTouchDown += ScatterViewItemMovedHandler;
                scatterItems[i].PreviewMouseDown += ScatterViewItemMovedHandler;

                // Bind the scatterviewitem opacity to our parent's opacity, so that we can fade out along with it
                Binding opacityBinding = new Binding("Opacity");
                opacityBinding.Source = this.Parent;
                scatterItems[i].SetBinding(ScatterViewItem.OpacityProperty, opacityBinding);
                
                physics.PositionLocked += ScatterViewItemLockedHandler;
                physics.HighPriority += ScatterViewItemHighPriorityHandler;
                physics.LowPriority += ScatterViewItemLowPriorityHandler;
                
                scatterItems[i].Center = this.Center;
                scatterItems[i].Orientation = this.Orientation;

                if(scatterItems[i] is DocumentScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_DOCUMENT_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_DOCUMENT_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_DOCUMENT_STARTING_SIZE.Height;
                }
                else if (scatterItems[i] is ImageContainerScatterItem || scatterItems[i] is ImageScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_IMAGE_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_IMAGE_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_IMAGE_STARTING_SIZE.Height;
                }
                else
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_VIDEO_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_VIDEO_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_VIDEO_STARTING_SIZE.Height;
                }

                view.Items.Add(scatterItems[i]);
                physicsItemsActive.Add(physics);
            }

            CalculateNewPositions(physicsItemsActive);
            physicsTimer.Start();
        }

        #endregion

        public TagWindow()
        {
            InitializeComponent();

            physicsTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            physicsTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            physicsTimer.Tick += PhysicsEventHandler;

            physicsTimerLowPriority = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            physicsTimerLowPriority.Interval = new TimeSpan(0, 0, 0, 0, 200);
            physicsTimerLowPriority.Tick += PhysicsLowPriorityEventHandler;

            animationPulseTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            animationPulseTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            animationPulseTimer.Tick += AnimationPulseHandler;
            animationPulseTimer.Start();
            animationPulse.Opacity = 0.6d;
        }
    }
}
