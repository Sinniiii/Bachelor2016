using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel.Model;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for ImageScatterItem.xaml
    /// </summary>
    public partial class ImageContainerScatterItem : ScatterViewItem
    {
        #region Fields

        List<BitmapImage> images;

        #endregion

        #region Members

        #endregion

        #region EventHandlers

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

        public void BarLoadedHandler(object obj, EventArgs args)
        {
            container.Populate(images, imageContainerControl, System.Windows.Controls.Orientation.Horizontal, 5, false);
        }

        public void NewMainImageHandler(ImageSource source)
        {
            mainImage.Source = source;
        }

        #endregion

        public ImageContainerScatterItem(List<SmartCardDataItem> imageItems)
        {
            InitializeComponent();

            images = new List<BitmapImage>();
            foreach (SmartCardDataItem item in imageItems)
                images.Add(item.GetImageSource());

            mainImage.Source = images[0];

            container.Loaded += BarLoadedHandler;
            container.NewMainImage += NewMainImageHandler;
        }
    }
}
