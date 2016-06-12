using System;
using System.Windows;
using System.Linq;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel;
using DatabaseModel.Model;
using Product_Browser.ScatterItems;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

//Baard was here

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        #region Fields

        static double
            MAX_REMOVE_AREA_SIZE = Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height * 0.05d,
            BACKGROUND_FADE_AMOUNT = 0.01d;

        ObservableCollection<ABBScatterItem> scatterItemCollection = new ObservableCollection<ABBScatterItem>();

        Random randomGenerator = new Random();

        DispatcherTimer removeAreaAnimationTimer, backgroundFadeTimer;

        int manipulatedSmartCards = 0;
        byte colorsUsed = 0;

        #endregion

        #region EventHandlers

        private void RemoveAreaAnimationHandler(object sender, EventArgs args)
        {
            if (manipulatedSmartCards > 0)
            {
                if (removeArea1.Height.Value >= MAX_REMOVE_AREA_SIZE)
                    return;

                removeArea1.Height = new GridLength(removeArea1.Height.Value + 5d);
                removeArea2.Height = new GridLength(removeArea2.Height.Value + 5d);
            }
            else
            {
                if (removeArea1.Height.Value == 0)
                    removeAreaAnimationTimer.Stop();
                else
                {
                    double newValue = removeArea1.Height.Value - 5d;

                    if (newValue < 0)
                        newValue = 0d;

                    removeArea1.Height = new GridLength(newValue);
                    removeArea2.Height = new GridLength(newValue);
                }
            }
        }

        private void SmartCardManipulationStarted(object sender, EventArgs args)
        {
            manipulatedSmartCards++;

            if (!removeAreaAnimationTimer.IsEnabled)
                removeAreaAnimationTimer.Start();
        }

        private void SmartCardManipulationEnded(object sender, EventArgs args)
        {
            manipulatedSmartCards--;
        }

        private void OnSmartCardSelected(object sender, EventArgs args)
        {
            VirtualSmartCardScatterItem x = new VirtualSmartCardScatterItem(scatterItemCollection, (sender as SmartCard).TagId);
            scatterItemCollection.Add(x);
            
            scatterView.UpdateLayout();
            x.InitializeVirtualSmartCard(GetNextColorTheme(), scatterView);

            x.ContainerManipulationStarted += SmartCardManipulationStarted;
            x.ContainerManipulationCompleted += SmartCardManipulationEnded;

            x.Center = new Point(Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width / 2d, Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height / 2d);
            x.Orientation = 0d;
        }

        private void OnSmartCardContainerLoaded(object sender, EventArgs args)
        {
            ABBDataContext con = new ABBDataContext();
            List<SmartCard> activeList = con.SmartCards.Where(d => d.DataItems.Count > 0).ToList();
            smartCardContainer.Populate(activeList);
        }

        private void OnVisualizationAdded(object sender, TagVisualizerEventArgs args)
        {
            TagVisualizationMod tagVis = args.TagVisualization as TagVisualizationMod;

            VirtualSmartCardScatterItem item = null;

            foreach (ScatterViewItem it in scatterView.Items) // Find out if we already have virtual version of this card placed
            {
                if (it is VirtualSmartCardScatterItem)
                {
                    if ((it as VirtualSmartCardScatterItem).TagId == tagVis.VisualizedTag.Value)
                        item = it as VirtualSmartCardScatterItem;
                }
            }

            if (item == null)
            {
                item = new VirtualSmartCardScatterItem(scatterItemCollection, tagVis.VisualizedTag.Value);

                scatterItemCollection.Add(item);
                scatterView.UpdateLayout(); // Force an immediate update

                item.InitializeVirtualSmartCard(GetNextColorTheme(), scatterView);

                // Assign our event handlers to input, so we can show the removal area
                item.ContainerManipulationStarted += SmartCardManipulationStarted;
                item.ContainerManipulationCompleted += SmartCardManipulationEnded;
            }

            (args.TagVisualization as TagVisualizationMod).InitializeSmartCard(item);
        }

        private void BackgroundFadeTick(object sender, EventArgs args)
        {
            double newOpacity = background.Opacity - BACKGROUND_FADE_AMOUNT;

            if (newOpacity < 0d)
                newOpacity = 0d;

            background.Opacity = newOpacity;

            if(newOpacity == 0d)
                backgroundFadeTimer.Stop();
        }

        private void BackgroundShowTick(object sender, EventArgs args)
        {
            double newOpacity = background.Opacity + BACKGROUND_FADE_AMOUNT;

            if (newOpacity > 1d)
                newOpacity = 1d;

            background.Opacity = newOpacity;

            if (newOpacity == 1d)
                backgroundFadeTimer.Stop();
        }

        private void ScatterItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if(args.Action == NotifyCollectionChangedAction.Remove && scatterItemCollection.Count == 0)
            {
                backgroundFadeTimer.Tick -= BackgroundFadeTick;
                backgroundFadeTimer.Tick += BackgroundShowTick;
                backgroundFadeTimer.Start();
            }
            else if(args.Action == NotifyCollectionChangedAction.Add && scatterItemCollection.Count == 1)
            {
                backgroundFadeTimer.Tick += BackgroundFadeTick;
                backgroundFadeTimer.Tick -= BackgroundShowTick;
                backgroundFadeTimer.Start();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds definitions for the Tag Visualizer item, which makes it be on the lookout for those tags being
        /// placed on the board.
        /// </summary>
        private void InitializeTagVisualizer()
        {
            for (int i = 0; i < 256; i++) // For now just add definitions for all tag ids
            {
                TagVisualizationDefinition tagDef = new TagVisualizationDefinition();

                tagDef.Source = new Uri("TagVisualizationMod.xaml", UriKind.Relative);
                tagDef.MaxCount = 1;
                tagDef.LostTagTimeout = 2000;
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
                tagDef.Value = i;
                tagDef.OrientationOffsetFromTag = 270d;
                tagDef.PhysicalCenterOffsetFromTag = new Vector(0.05d, 0.05d);

                tagVisualizer.Definitions.Add(tagDef);
            }

            // Subscribe to add event, since we need to handle that one
            tagVisualizer.VisualizationAdded += OnVisualizationAdded;
        }

        private Color GetNextColorTheme()
        {
            // Original #2a5f6f R = 42, G = 95, B = 111, A = 255

            Color newColor = new Color();
            newColor.R = (byte)randomGenerator.Next(10, 70);
            newColor.G = (byte)randomGenerator.Next(60, 130);
            newColor.B = (byte)randomGenerator.Next(80, 140);
            newColor.A = 255;

            return newColor;
        }

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeTagVisualizer();
            
            removeAreaAnimationTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            removeAreaAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            removeAreaAnimationTimer.Tick += RemoveAreaAnimationHandler;

            backgroundFadeTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            backgroundFadeTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            smartCardContainer.Loaded += OnSmartCardContainerLoaded;
            smartCardContainer.SmartCardSelected += OnSmartCardSelected;

            scatterView.ItemsSource = scatterItemCollection;

            scatterItemCollection.CollectionChanged += ScatterItemCollectionChanged;
        }
    }
}
