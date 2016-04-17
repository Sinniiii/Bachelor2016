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
            SCATTERITEM_STARTING_WIDTH = 200d,
            SCATTERITEM_STARTING_HEIGHT = 100d;


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
        List<ScatterItemPhysics> physicsItemsInactive = new List<ScatterItemPhysics>();

        DispatcherTimer physicsTimer;

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

        public TagWindow()
        {
            InitializeComponent();
        }

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
            if (physicsItemsActive.Count == 0)
                physicsTimer.Start();

            ScatterItemPhysics phy = null;
            foreach(ScatterItemPhysics p in physicsItemsInactive)
            {
                if (p.Item == sender as ScatterViewItem)
                {
                    phy = p;
                    physicsItemsActive.Add(p);
                    break;
                }
            }

            if (phy != null) // If it's null, it already exists in the active container
                physicsItemsInactive.Remove(phy);
        }

        private void ScatterViewItemLockedHandler(ScatterItemPhysics e)
        {
            physicsItemsInactive.Add(e);
            physicsItemsActive.Remove(e);

            if (physicsItemsActive.Count == 0)
                physicsTimer.Stop();
        }

        private void PhysicsEventHandler(object sender, EventArgs args)
        {
            for(int i = 0; i < physicsItemsActive.Count; i++)
                physicsItemsActive[i].Run(Center, Orientation, -50d, 150d);
        }

        public void DestroySmartCard(ScatterView view)
        {
            for (int i = 0; i < physicsItemsInactive.Count; i++)
                view.Items.Remove(physicsItemsInactive[i].Item);
            for (int i = 0; i < physicsItemsActive.Count; i++)
                view.Items.Remove(physicsItemsActive[i].Item);

            physicsItemsActive.Clear();
            physicsItemsInactive.Clear();
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

            double radianMultiplier = Math.PI / (dataItems.Count + 1); // Radians so PI = 180 degrees
            for(int i = 0; i < dataItems.Count; i++)
            {
                ScatterViewItem item = null;
                switch (dataItems[i].Category)
                {
                    case SmartCardDataItemCategory.Document:
                        item = new DocumentScatterItem(dataItems[i]);
                        break;
                    case SmartCardDataItemCategory.Image:
                        item = new ImageScatterItem(dataItems[i]); // Temporary, for final all images should be sent to same imagescatteritem
                        break;
                    case SmartCardDataItemCategory.Video:
                        item = new VideoScatterItem(dataItems[i]);
                        break;
                }

                // Calculate position, place items on half circle
                double positionRadians = radianMultiplier * (i + 1) + Math.PI;

                double posXOffset = Math.Cos(positionRadians) * CIRCLE_SIZE;
                double posYOffset = Math.Sin(positionRadians) * CIRCLE_SIZE + CIRCLE_Y_OFFSET;

                ScatterItemPhysics physics = new ScatterItemPhysics(item, posXOffset, posYOffset, positionRadians * 57.295d, // Convert that one to degrees
                    SCATTERITEM_STARTING_WIDTH, SCATTERITEM_STARTING_HEIGHT);

                physics.ResetToDefault(Center, Orientation);

                item.PreviewTouchDown += ScatterViewItemMovedHandler;
                item.PreviewMouseLeftButtonDown += ScatterViewItemMovedHandler;

                physics.PositionLocked += ScatterViewItemLockedHandler;

                physicsItemsInactive.Add(physics);
                view.Items.Add(item);
            }

            physicsTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            physicsTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            physicsTimer.Tick += PhysicsEventHandler;
            physicsTimer.Start();
        }
    }
}
