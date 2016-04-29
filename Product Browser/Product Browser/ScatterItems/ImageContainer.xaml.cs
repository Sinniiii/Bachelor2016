using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Controls;

namespace Product_Browser.ScatterItems
{
    public enum DisplayState
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Interaction logic for ImageContainer.xaml
    /// </summary>
    public partial class ImageContainer : UserControl
    {
        private const double SCROLL_SPEED = 10d;

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Fields

        DisplayState displayState = DisplayState.Horizontal;

        List<BitmapSource> images;

        /// <summary>
        /// For automatically scrolling to closest item point
        /// </summary>
        double[] stopOffsetPoints;
        double currentOffsetTarget;
        DispatcherTimer scrollTimer;

        /// <summary>
        /// Used by mouse/touch scrolling
        /// </summary>
        Point scrollStartPoint;
        double scrollStartXOffset;

        #endregion

        #region Properties



        #endregion

        #region Events

        public delegate void NewMainImageHandler(ImageSource source);
        public event NewMainImageHandler NewMainImage;

        #endregion

        #region EventHandlers

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            scrollStartPoint = e.GetPosition(this);
            scrollStartXOffset = scrollBar.HorizontalOffset;
            
            CaptureMouse();
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);

            scrollStartPoint = e.GetTouchPoint(this).Position;
            scrollStartXOffset = scrollBar.HorizontalOffset;

            e.TouchDevice.Capture(this);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (IsMouseCaptured)
            {
                Point point = e.GetPosition(this);

                Point delta = new Point(point.X - scrollStartPoint.X, 0d);

                scrollBar.ScrollToHorizontalOffset(-delta.X + scrollStartXOffset);
            }
        }

        protected override void OnPreviewTouchMove(TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);

            if (e.TouchDevice.Captured == this)
            {
                Point point = e.GetTouchPoint(this).Position;

                Point delta = new Point(point.X - scrollStartPoint.X, 0d);

                scrollBar.ScrollToHorizontalOffset(-delta.X + scrollStartXOffset);
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();

            StartAutoScroll();
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == this)
                ReleaseTouchCapture(e.TouchDevice);

            StartAutoScroll();
        }

        protected void OnScrollTimer(object obj, EventArgs args)
        {
            double delta = currentOffsetTarget - scrollBar.HorizontalOffset;
            double speed = delta > 0 ? SCROLL_SPEED : -SCROLL_SPEED;

            if (Math.Abs(speed) > Math.Abs(delta))
                speed = delta;

            scrollBar.ScrollToHorizontalOffset(scrollBar.HorizontalOffset + speed);

            // Check if we arrived
            if(scrollBar.HorizontalOffset > currentOffsetTarget - 0.1d && scrollBar.HorizontalOffset < currentOffsetTarget + 0.1d)
            {
                // Find the correct image
                int desiredImageIndex = 0;
                for(int i = 0; i < stopOffsetPoints.Length; i++)
                {
                    if(stopOffsetPoints[i] > scrollBar.HorizontalOffset - 0.1d && stopOffsetPoints[i] < scrollBar.HorizontalOffset + 0.1d)
                    {
                        desiredImageIndex = i;
                        break;
                    }
                }

                // Send the event(add 1 to index, due to starting with an empty image as space occupier
                NewMainImage(((Image)stackPanel.Children[desiredImageIndex + 1]).Source);

                scrollTimer.Stop();
            }
        }

        protected void OnParentSizeChanged(object obj, SizeChangedEventArgs args)
        {
            RecalculateSize(args.PreviousSize.Width, args.NewSize.Width);
        }

        #endregion

        #region Methods

        protected void StartAutoScroll()
        {
            // Calculate closest stop point
            double closestDistance = 999999999d;
            int closestIndex = -1;
            
            for(int i = 0; i < stopOffsetPoints.Length; i++)
            {
                double distance = stopOffsetPoints[i] - scrollBar.HorizontalOffset;
                if (Math.Abs(distance) < Math.Abs(closestDistance))
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            currentOffsetTarget = stopOffsetPoints[closestIndex];

            scrollTimer.Start();
        }

        private void RecalculateSize(double oldWidth, double newWidth)
        {
            int childrenCount = stackPanel.Children.Count;

            stopOffsetPoints = new double[childrenCount - 2];

            double imageWidth = newWidth / 3d; // Three images appear at a time
            for(int i = 0; i < childrenCount; i++)
            {
                ((Image)stackPanel.Children[i]).Width = imageWidth;

                if (i == 0 || i == childrenCount - 1) // First and last images are empty space occupiers
                    continue;

                // For the others, populate the stop points
                int newIndex = i - 1;
                stopOffsetPoints[newIndex] = newIndex * imageWidth;
            }

            if (oldWidth != 0d)
            {
                double oldNewRatio = newWidth / oldWidth;
                scrollBar.ScrollToHorizontalOffset(scrollBar.HorizontalOffset * oldNewRatio);
            }
        }

        public void Populate(List<BitmapImage> images, ScatterViewItem item)
        {
            item.SizeChanged += OnParentSizeChanged; // Subscribe to parent size change event, need to adjust image sizes

            Image empty = new Image();
            stackPanel.Children.Add(empty);
            
            foreach(ImageSource s in images)
            {
                Image child = new Image();
                child.Source = s;
                child.Stretch = Stretch.Uniform;
                child.HorizontalAlignment = HorizontalAlignment.Stretch;
                child.VerticalAlignment = VerticalAlignment.Stretch;
                stackPanel.Children.Add(child);
            }

            empty = new Image();
            stackPanel.Children.Add(empty);

            RecalculateSize(0d, item.Width);
        }

        #endregion

        public ImageContainer()
        {
            InitializeComponent();

            scrollTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            scrollTimer.Tick += OnScrollTimer;
        }
    }
}
