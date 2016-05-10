using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using Product_Browser.ScatterItems;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for TagVisualizationMod.xaml
    /// </summary>
    public partial class TagVisualizationMod : TagVisualization
    {
        private VirtualSmartCardScatterItem connectedItem;

        protected override void OnMoved(RoutedEventArgs e)
        {
            base.OnMoved(e);

            if(connectedItem != null)
            {
                connectedItem.Center = this.Center;
                connectedItem.Orientation = this.Orientation;

                connectedItem.Moved();
            }
        }

        public void InitializeSmartCard(VirtualSmartCardScatterItem connectedItem)
        {
            this.connectedItem = connectedItem;

            connectedItem.Center = this.Center;
            connectedItem.Orientation = this.Orientation;

            connectedItem.Moved();
        }

        public TagVisualizationMod()
        {
            InitializeComponent();
        }
    }
}
