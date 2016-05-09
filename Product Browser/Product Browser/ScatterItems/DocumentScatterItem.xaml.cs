using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel.Model;
using Microsoft.Surface.Presentation.Input;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for DocumentScatterItem.xaml
    /// </summary>
    public partial class DocumentScatterItem : ScatterViewItem
    {
        List<BitmapImage> images;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
            e.Handled = false; // Continue upwards, to notify tagWindow of movement
        }

        public void OnBarLoaded(object obj, EventArgs args)
        {
            container.Populate(images, imageContainerControl, System.Windows.Controls.Orientation.Vertical, 5, true);
        }

        public void OnNewMainImage(ImageSource source)
        {
            mainImage.Source = source;
        }

        public DocumentScatterItem(SmartCardDataItem document)
        {
            InitializeComponent();

            images = document.GetDocumentAsImageSources();

            mainImage.Source = images[0];

            container.Loaded += OnBarLoaded;
            container.NewMainImage += OnNewMainImage;
        }
    }
}
