using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for ScatterViewMod.xaml
    /// </summary>
    public partial class ScatterViewMod : ScatterView
    {
        public ScatterViewMod()
        {
            InitializeComponent();
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);

            //// Only let fingers manipulate scatterviewitems
            if (!e.TouchDevice.GetIsFingerRecognized())
                e.Handled = true;
        }
    }
}
