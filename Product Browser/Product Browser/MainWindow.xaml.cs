using System;
using System.Data.Entity;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Surface.Core;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using DatabaseModel;
using DatabaseModel.Model;
using TouchEventArgs = System.Windows.Input.TouchEventArgs;
using Tesseract;
using System.Windows.Input;

//Baard was here

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeTagVisualizer();
        }

        /// <summary>
        /// Adds definitions for the Tag Visualizer item, which makes it be on the lookout for those tags being
        /// placed on the board.
        /// </summary>
        private void InitializeTagVisualizer()
        {
            for (int i = 0; i < 256; i++) // For now just add definitions for all tag ids
            {
                TagVisualizationDefinition tagDef = new TagVisualizationDefinition();

                tagDef.Source = new Uri("TagWindow.xaml", UriKind.Relative);
                tagDef.MaxCount = 1;
                tagDef.LostTagTimeout = 2000;
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
                tagDef.Value = i;
                tagDef.OrientationOffsetFromTag = 270d;
                tagDef.PhysicalCenterOffsetFromTag = new Vector(0.05d, 0.05d);

                tagVisualizer.Definitions.Add(tagDef);
            }

            // Subscribe to add/remove events, since we need to handle those
            tagVisualizer.VisualizationInitialized += VisualizationAdded;
            tagVisualizer.VisualizationRemoved += VisualizationRemoved;
        }

        private void VisualizationAdded(object sender, TagVisualizerEventArgs args)
        {
            (args.TagVisualization as TagWindow).InitializeSmartCard(scatterView);
        }

        private void VisualizationRemoved(object sender, TagVisualizerEventArgs args)
        {
            (args.TagVisualization as TagWindow).DestroySmartCard(scatterView);
        }
    }
}
