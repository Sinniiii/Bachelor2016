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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Surface.Core;
using System.Windows.Data;
using System.IO;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class VirtualSmartCardScatterItem : ScatterViewItem, INotifyPropertyChanged
    {

        #region Physics, position and graphics constants

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

        readonly double
            ANIMATION_PULSE_1_STARTING_OPACITY = 0.05d,
            ANIMATION_PULSE_1_OPACITY_CHANGE = 0.01d,
            ANIMATION_PULSE_1_MAX_OPACITY = 0.65,
            ANIMATION_PULSE_1_MIN_OPACITY = 0.05d,

            ANIMATION_PULSE_2_STARTING_OPACITY = 0.05d,
            ANIMATION_PULSE_2_OPACITY_CHANGE = 0.01d,
            ANIMATION_PULSE_2_MAX_OPACITY = 0.65,
            ANIMATION_PULSE_2_MIN_OPACITY = 0.05d;


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

        bool pulseUp1 = true, pulseUp2 = true;

        /// <summary>
        /// These are helpers to determine visibility of UI elements in TagWindow.xaml. Start by displaying loading
        /// </summary>
        bool foundSmartCard = false,
            notFoundSmartCard = false,
            loadingSmartCard = true;

        #endregion

        #region Properties

        public long TagId { get; private set; }

        /// <summary>
        /// These are states, setting one to true disables the others
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

        /// <summary>
        /// Overwritten to ensure we do not capture touch with anything but center object
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTouchDown(System.Windows.Input.TouchEventArgs e)
        {
            if (centerObject.TouchesOver.Contains(e.TouchDevice))
                base.OnTouchDown(e);
            else
                e.Handled = true;
        }

        /// <summary>
        /// Overwritten to ensure we do not capture touch with anything but center object
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (centerObject.IsMouseOver)
                base.OnMouseDown(e);
            else
                e.Handled = true;
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            Moved();
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
            
            e.Item.Deceleration = double.NaN; // Prevent inertia

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
                if (physicsItemsActive[i].Run(ActualCenter, ActualOrientation))
                    toDiscard.Add(physicsItemsActive[i]);

            physicsItemsActive.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void PhysicsLowPriorityEventHandler(object sender, EventArgs args)
        {
            List<ScatterItemPhysics> toDiscard = new List<ScatterItemPhysics>(physicsItemsActive.Count);

            for (int i = 0; i < physicsItemsActiveLowPriority.Count; i++)
                if (physicsItemsActiveLowPriority[i].RunLowPriority(ActualCenter, ActualOrientation, CIRCLE_SIZE))
                    toDiscard.Add(physicsItemsActiveLowPriority[i]);

            physicsItemsActiveLowPriority.RemoveAll(a => toDiscard.Contains(a));

            if (physicsItemsActiveLowPriority.Count == 0)
                physicsTimerLowPriority.Stop();
        }

        private void AnimationPulseHandler(object sender, EventArgs args)
        {
            if (pulseUp1)
            {
                animationPulse1.Opacity += ANIMATION_PULSE_1_OPACITY_CHANGE;

                if (animationPulse1.Opacity >= ANIMATION_PULSE_1_MAX_OPACITY)
                    pulseUp1 = false;
            }
            else
            {
                animationPulse1.Opacity -= ANIMATION_PULSE_1_OPACITY_CHANGE;

                if (animationPulse1.Opacity <= ANIMATION_PULSE_1_MIN_OPACITY)
                    pulseUp1 = true;
            }

            if (pulseUp2)
            {
                animationPulse2.Opacity += ANIMATION_PULSE_2_OPACITY_CHANGE;

                if (animationPulse2.Opacity >= ANIMATION_PULSE_2_MAX_OPACITY)
                    pulseUp2 = false;
            }
            else
            {
                animationPulse2.Opacity -= ANIMATION_PULSE_2_OPACITY_CHANGE;

                if (animationPulse2.Opacity <= ANIMATION_PULSE_2_MIN_OPACITY)
                    pulseUp2 = true;
            }
        }

        #endregion

        #region Methods

        public void Moved()
        {
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

            // Ensure this is below the others
            this.ZIndex = -1;
        }

        private async void InitializeVirtualSmartCard(ScatterView view)
        {
            ABBDataContext context = new ABBDataContext();
            
            smartCard = await context.SmartCards
                .Include(s => s.DataItems.Select(d => d.DataField))
                .FirstOrDefaultAsync(a => a.TagId == TagId);
            
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
                        if(File.Exists(SmartCardDataItem.VIDEO_FOLDER + dataItems[i].Name)) // Don't add if we cant find video
                            item = new VideoScatterItem(dataItems[i]);
                        break;
                }

                if(item != null)
                    scatterItems.Add(item);
            }

            for (int i = 0; i < scatterItems.Count; i++)
            {
                ScatterItemPhysics physics = new ScatterItemPhysics(scatterItems[i]);

                scatterItems[i].TouchDown += ScatterViewItemMovedHandler;
                scatterItems[i].MouseDown += ScatterViewItemMovedHandler;
                
                physics.PositionLocked += ScatterViewItemLockedHandler;
                physics.HighPriority += ScatterViewItemHighPriorityHandler;
                physics.LowPriority += ScatterViewItemLowPriorityHandler;

                scatterItems[i].Center = this.ActualCenter;
                scatterItems[i].Orientation = this.ActualOrientation;

                if(scatterItems[i] is DocumentScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_DOCUMENT_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_DOCUMENT_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_DOCUMENT_STARTING_SIZE.Height;
                    physics.PullOffset = (Point)SCATTERITEM_DOCUMENT_STARTING_POSITION;

                    documentPlaceholder.Visibility = Visibility.Visible;
                }
                else if (scatterItems[i] is ImageContainerScatterItem || scatterItems[i] is ImageScatterItem)
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_IMAGE_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_IMAGE_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_IMAGE_STARTING_SIZE.Height;
                    physics.PullOffset = (Point)SCATTERITEM_IMAGE_STARTING_POSITION;

                    imagePlaceholder.Visibility = Visibility.Visible;
                }
                else
                {
                    physics.OriginalOrientationOffset = SCATTERITEM_VIDEO_STARTING_ROTATION;
                    physics.OriginalWidth = SCATTERITEM_VIDEO_STARTING_SIZE.Width;
                    physics.OriginalHeight = SCATTERITEM_VIDEO_STARTING_SIZE.Height;
                    physics.PullOffset = (Point)SCATTERITEM_VIDEO_STARTING_POSITION;

                    videoPlaceholder.Visibility = Visibility.Visible;
                }

                view.Items.Add(scatterItems[i]);
                physicsItemsActive.Add(physics);
            }

            CalculateNewPositions(physicsItemsActive);
            physicsTimer.Start();
        }

        #endregion

        public VirtualSmartCardScatterItem(ScatterView view, long tagId)
        {
            InitializeComponent();

            TagId = tagId;

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

            animationPulse1.Opacity = ANIMATION_PULSE_1_STARTING_OPACITY;
            animationPulse2.Opacity = ANIMATION_PULSE_2_STARTING_OPACITY;

            this.CanScale = false; // Disable resizing
            
            ShowsActivationEffects = false; // Disable flash when selected
            IsTopmostOnActivation = false; // Prevent it from showing over the other elements

            InitializeVirtualSmartCard(view);
        }
    }
}
