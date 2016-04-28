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

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for ImageScatterItem.xaml
    /// </summary>
    public partial class ImageScatterItem : ScatterViewItem
    {
        #region Fields

        List<BitmapImage> images;

        #endregion

        #region Members

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!container.IsMouseOver) // Else we capture with imagecontainer
                base.OnMouseDown(e);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            if (!container.TouchesOver.Contains(e.TouchDevice)) // Else we capture with imagecontainer
                base.OnTouchDown(e);
            else
                e.Handled = true;
        }

        public void OnBarLoaded(object obj, EventArgs args)
        {
            container.Populate(images, this);
        }

        public void OnNewMainImage(ImageSource source)
        {
            mainImage.Source = source;
        }

        public ImageScatterItem(List<SmartCardDataItem> imageItems)
        {
            InitializeComponent();

            images = new List<BitmapImage>();
            foreach (SmartCardDataItem item in imageItems)
                images.Add(item.GetImageSource());

            mainImage.Source = images[0];

            container.Loaded += OnBarLoaded;
            container.NewMainImage += OnNewMainImage;
        }
    }
}
