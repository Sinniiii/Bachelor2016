using Microsoft.Surface.Presentation.Controls;

namespace TagExpansionPrototype
{
    /// <summary>
    /// Interaction logic for ScatterViewItemDummy.xaml
    /// </summary>
    public partial class TagVisualizationDummy : TagVisualization
    {

        public CombinedTagScatterItem ConnectedCombinedTag { get; set; }

        public TagVisualizationDummy()
        {
            InitializeComponent();
        }
    }
}
