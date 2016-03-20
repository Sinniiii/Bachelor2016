using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace ProductBrowser.Resources
{
    [ExportMetadata("Culture", "en")]
    [Export(typeof(ResourceDictionary))]
    public partial class EnglishLanguage : ResourceDictionary
    {
        public EnglishLanguage()
        {
            InitializeComponent();
        }
    }
}
