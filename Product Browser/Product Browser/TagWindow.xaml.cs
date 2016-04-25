using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel;
using DatabaseModel.Model;
using System.Data.Entity;
using Product_Browser.ScatterItems;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Surface.Core;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : TagVisualization, INotifyPropertyChanged
    {
        const double
            CIRCLE_Y_OFFSET = -50d,
            CIRCLE_SIZE = 150d,
            SCATTERITEM_STARTING_WIDTH = 100d,
            SCATTERITEM_STARTING_HEIGHT = 200d;
        
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

        List<ScatterItemPhysics> physicsItemsActive = new List<ScatterItemPhysics>();
        List<ScatterItemPhysics> physicsItemsActiveLowPriority = new List<ScatterItemPhysics>();
        List<ScatterItemPhysics> physicsItemsInactive = new List<ScatterItemPhysics>();

        DispatcherTimer physicsTimer;
        DispatcherTimer physicsTimerLowPriority;

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

            foreach (ScatterItemPhysics item in physicsItemsInactive)
            {
                item.ResetToDefault(Center, Orientation);
            }
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
            physicsItemsActive.Remove(e);

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void ScatterViewItemLowPriorityHandler(ScatterItemPhysics e)
        {
            if (!physicsTimerLowPriority.IsEnabled)
                physicsTimerLowPriority.Start();

            physicsItemsActiveLowPriority.Add(e);
            physicsItemsActive.Remove(e);
        }

        private void ScatterViewItemHighPriorityHandler(ScatterItemPhysics e)
        {
            if (!physicsTimer.IsEnabled)
                physicsTimer.Start();

            // Remove from low priority container
            physicsItemsActiveLowPriority.Remove(e);

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
            for(int i = 0; i < physicsItemsActive.Count; i++)
                physicsItemsActive[i].Run(Center, Orientation, -50d, 175d);

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void PhysicsLowPriorityEventHandler(object sender, EventArgs args)
        {
            for (int i = 0; i < physicsItemsActiveLowPriority.Count; i++)
                physicsItemsActiveLowPriority[i].RunLowPriority(Center, Orientation, -50d, 175d);

            if (physicsItemsActiveLowPriority.Count == 0)
                physicsTimerLowPriority.Stop();
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
            double radianMultiplier = Math.PI / (items.Count + 1); // Radians so PI = 180 degrees

            for (int i = 0; i < items.Count; i++)
            {
                // Calculate position, place items on half circle
                double positionRadians = radianMultiplier * (i + 1) + Math.PI;

                double posXOffset = Math.Cos(positionRadians) * CIRCLE_SIZE;
                double posYOffset = Math.Sin(positionRadians) * CIRCLE_SIZE + CIRCLE_Y_OFFSET;

                items[i].OriginalPositionOffset = new Point(posXOffset, posYOffset);
                items[i].OriginalOrientationOffset = positionRadians * 57.295d; // Convert to degrees
                items[i].OriginalWidth = SCATTERITEM_STARTING_WIDTH;
                items[i].OriginalHeight = SCATTERITEM_STARTING_HEIGHT;
            }
        }

        public async void InitializeSmartCard(ScatterView view)
        {
            ABBDataContext context = new ABBDataContext();
            
            smartCard = await context.SmartCards.FirstOrDefaultAsync(a => a.TagId == VisualizedTag.Value);
            
            List<SmartCardDataItem> dataItems = null;

            if (smartCard == null || (dataItems = smartCard.DataItems) == null || dataItems.Count == 0)
            {
                NotFoundSmartCard = true;
                return;
            }

            FoundSmartCard = true;

            List<ScatterViewItem> scatterItems = new List<ScatterViewItem>();

            // Place all images inside one scatter item
            dataItems = smartCard.DataItems.FindAll(a => a.Category == SmartCardDataItemCategory.Image);
            if(dataItems.Count != 0)
            {
                ScatterViewItem item = new ImageScatterItem(dataItems);
                scatterItems.Add(item);
            }

            // One scatter item each for everything else
            dataItems = smartCard.DataItems.FindAll(a => a.Category != SmartCardDataItemCategory.Image);
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

                physics.PositionLocked += ScatterViewItemLockedHandler;
                physics.HighPriority += ScatterViewItemHighPriorityHandler;
                physics.LowPriority += ScatterViewItemLowPriorityHandler;

                scatterItems[i].Center = this.Center;
                scatterItems[i].Orientation = this.Orientation;

                scatterItems[i].Width = SCATTERITEM_STARTING_WIDTH;
                scatterItems[i].Height = SCATTERITEM_STARTING_HEIGHT;

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
        }
    }
}
