using System;
using System.Windows;
using System.Linq;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

namespace TagExpansionPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        const double MAX_COMBINED_TAG_DISTANCE = 50d;

        protected void OnVisualizationAdded(object sender, TagVisualizerEventArgs args)
        {
            TagVisualizationDummy newVis = args.TagVisualization as TagVisualizationDummy;

            // Check existing tagsvisualizations to see if any are close by
            TagVisualizationDummy selected = null;
            foreach (TagVisualizationDummy dummy in tagVisualizer.ActiveVisualizations)
            {
                if (dummy == newVis || dummy.ConnectedCombinedTag != null)
                    continue;
                
                double distance = Math.Abs(((Vector)dummy.Center - (Vector)newVis.Center).Length);

                if(distance < MAX_COMBINED_TAG_DISTANCE)
                {
                    selected = dummy;
                    break;
                }
            }

            if(selected != null)
            {
                CombinedTagScatterItem item = new CombinedTagScatterItem(newVis, selected);
                scatterView.Items.Add(item);
            }
        }

        protected void OnTagVisualizerLoaded(object sender, EventArgs args)
        {
            // Add tag definitions
            for (int i = 0; i < 256; i++) // For now just add definitions for all tag ids
            {
                TagVisualizationDefinition tagDef = new TagVisualizationDefinition();
                
                tagDef.Source = new Uri("TagVisualizationDummy.xaml", UriKind.Relative);
                tagDef.MaxCount = 100;
                tagDef.LostTagTimeout = 2000;
                tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
                tagDef.Value = i;

                tagVisualizer.Definitions.Add(tagDef);
            }

            // Subscribe to add event, since we need to handle that one
            tagVisualizer.VisualizationAdded += OnVisualizationAdded;
        }

        public MainWindow()
        {
            InitializeComponent();

            tagVisualizer.Loaded += OnTagVisualizerLoaded;
        }
    }
}
