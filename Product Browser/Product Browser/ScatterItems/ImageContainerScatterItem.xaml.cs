using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel.Model;
using System.Windows.Threading;
using System.Windows;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for ImageScatterItem.xaml
    /// </summary>
    public partial class ImageContainerScatterItem : ABBScatterItem
    {
        #region Fields

        List<BitmapImage> images;

        DispatcherTimer effectTimer;

        Point originalAspectRatio, targetAspectRatio;

        #endregion

        #region EventHandlers

        public override void AnimationPulseHandler(object sender, EventArgs args)
        {
            grad.Angle = (grad.Angle + 0.5d) % 360d;
        }

        public void BarLoadedHandler(object obj, EventArgs args)
        {
            container.Populate(images, System.Windows.Controls.Orientation.Horizontal, 4, false, false);
            container.ColorTheme = GradientColor;
        }

        private void OnNewMainImage(BitmapImage source)
        {
            // Non-shader method
            //mainImage.Source = source;
            
            // Using effect shader
            if (originalAspectRatio.X == 0d)
            {
                transitionEffect.OldImage = new ImageBrush(source);

                originalAspectRatio = transitionEffect.AspectRatio = FindNormalizedAspectRatio(source);
            }
            else
            {
                transitionEffect.Input = new ImageBrush(source);

                targetAspectRatio = FindNormalizedAspectRatio(source);

                if (!effectTimer.IsEnabled)
                    transitionEffect.Progress = 0d;
                else
                    originalAspectRatio = transitionEffect.AspectRatio;

                effectTimer.Start();
            }
        }

        private void OnEffectTimer(object sender, EventArgs args)
        {
            transitionEffect.Progress += 0.05;

            transitionEffect.AspectRatio = originalAspectRatio + ((targetAspectRatio - originalAspectRatio) * transitionEffect.Progress);

            if (transitionEffect.Progress >= 1d)
            {
                transitionEffect.OldImage = transitionEffect.Input;
                transitionEffect.AspectRatio = originalAspectRatio = targetAspectRatio;
                targetAspectRatio = new Point();
                effectTimer.Stop();
            }
        }

        #endregion

        #region Methods

        private Point FindNormalizedAspectRatio(BitmapImage image)
        {
            double containerAspectRatio = mainImage.ActualHeight / mainImage.ActualWidth;
            double imageAspectRatio = (double)image.PixelHeight / image.PixelWidth;

            if(imageAspectRatio >= containerAspectRatio)
            {
                // Tall, height maxed
                double width = (mainImage.ActualHeight / imageAspectRatio);

                return new Point(width / mainImage.ActualWidth, 1d);
            }
            else
            {
                // Wide, width maxed
                double height = (mainImage.ActualWidth * imageAspectRatio);

                return new Point(1d, height / mainImage.ActualHeight);
            }
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
            container.NewMainImage += OnNewMainImage;

            effectTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            effectTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            effectTimer.Tick += OnEffectTimer;
        }
    }
}
