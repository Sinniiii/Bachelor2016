using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace ProductBrowser.Resources
{
    [ExportMetadata("Culture", "nb-NO")]
    [Export(typeof(ResourceDictionary))]
    public partial class NorwegianLanguage : ResourceDictionary
    {
        public NorwegianLanguage()
        {
            InitializeComponent();
        }
    }
}
