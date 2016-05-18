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
    public partial class ImageContainerScatterItem : ABBScatterItem
    {
        #region Fields

        List<BitmapImage> images;

        #endregion

        #region Members

        #endregion

        #region EventHandlers

        public override void AnimationPulseHandler(object sender, EventArgs args)
        {
            grad.Angle = (grad.Angle + 0.5d) % 360d;
        }

        public void BarLoadedHandler(object obj, EventArgs args)
        {
            container.Populate(images, System.Windows.Controls.Orientation.Horizontal, 4, false, false);
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
