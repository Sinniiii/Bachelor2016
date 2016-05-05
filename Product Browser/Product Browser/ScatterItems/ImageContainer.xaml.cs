using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private const double SCROLL_SPEED = 10d,
                             PAGE_NUMBER_OPACITY = 0.15d;
        private double numberOfImages = 3d;
        private int placeholderImages = 2;

        private bool populated = false;

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

        List<BitmapImage> images;

        /// <summary>
        /// For automatically scrolling to closest item point
        /// </summary>
        double[] stopOffsetPoints;
        double currentOffsetTarget;
        DispatcherTimer scrollTimer;

        /// <summary>
        /// Used by mouse/touch scrolling
        /// </summary>
        double  scrollStartPoint,
                scrollStartOffset;

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

            if (stackPanel.Orientation == Orientation.Horizontal)
            {
                scrollStartPoint = e.GetPosition(this).X;
                scrollStartOffset = scrollBar.HorizontalOffset;
            }
            else
            {
                scrollStartPoint = e.GetPosition(this).Y;
                scrollStartOffset = scrollBar.VerticalOffset;
            }
            
            CaptureMouse();
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);

            if (stackPanel.Orientation == Orientation.Horizontal)
            {
                scrollStartPoint = e.GetTouchPoint(this).Position.X;
                scrollStartOffset = scrollBar.HorizontalOffset;
            }
            else
            {
                scrollStartPoint = e.GetTouchPoint(this).Position.Y;
                scrollStartOffset = scrollBar.VerticalOffset;
            }

            e.TouchDevice.Capture(this);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (IsMouseCaptured)
            {
                Point point = e.GetPosition(this);
                
                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    double delta = point.X - scrollStartPoint;

                    scrollBar.ScrollToHorizontalOffset(-delta + scrollStartOffset);
                }
                else
                {
                    double delta = point.Y - scrollStartPoint;

                    scrollBar.ScrollToVerticalOffset(-delta + scrollStartOffset);
                }
            }
        }

        protected override void OnPreviewTouchMove(TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);

            if (e.TouchDevice.Captured == this)
            {
                Point point = e.GetTouchPoint(this).Position;

                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    double delta = point.X - scrollStartPoint;

                    scrollBar.ScrollToHorizontalOffset(-delta + scrollStartOffset);
                }
                else
                {
                    double delta = point.Y - scrollStartPoint;

                    scrollBar.ScrollToVerticalOffset(-delta + scrollStartOffset);
                }
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
            double delta = 0d;
            
            if(stackPanel.Orientation == Orientation.Horizontal)
                delta = currentOffsetTarget - scrollBar.HorizontalOffset;
            else
                delta = currentOffsetTarget - scrollBar.VerticalOffset;

            double speed = delta > 0 ? SCROLL_SPEED : -SCROLL_SPEED;

            // Don't overshoot
            if (Math.Abs(speed) > Math.Abs(delta))
                speed = delta;

            if (stackPanel.Orientation == Orientation.Horizontal) // TODO: Wasteful, fix
            {
                scrollBar.ScrollToHorizontalOffset(scrollBar.HorizontalOffset + speed);

                // Check if we arrived
                if (scrollBar.HorizontalOffset > currentOffsetTarget - 0.1d && scrollBar.HorizontalOffset < currentOffsetTarget + 0.1d)
                {
                    // Find the correct image
                    int desiredImageIndex = 0;
                    for (int i = 0; i < stopOffsetPoints.Length; i++)
                    {
                        if (stopOffsetPoints[i] > scrollBar.HorizontalOffset - 0.1d && stopOffsetPoints[i] < scrollBar.HorizontalOffset + 0.1d)
                        {
                            desiredImageIndex = i;
                            break;
                        }
                    }

                    // Send the event(add placeholderimages/2 to index, due to starting with empty images as space occupiers
                    NewMainImage(((Image)((Grid)((UserControl)stackPanel.Children[desiredImageIndex + (placeholderImages / 2)]).Content).Children[0]).Source);

                    scrollTimer.Stop();
                }
            }
            else
            {
                scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset + speed);

                // Check if we arrived
                if (scrollBar.VerticalOffset > currentOffsetTarget - 0.1d && scrollBar.VerticalOffset < currentOffsetTarget + 0.1d)
                {
                    // Find the correct image
                    int desiredImageIndex = 0;
                    for (int i = 0; i < stopOffsetPoints.Length; i++)
                    {
                        if (stopOffsetPoints[i] > scrollBar.VerticalOffset - 0.1d && stopOffsetPoints[i] < scrollBar.VerticalOffset + 0.1d)
                        {
                            desiredImageIndex = i;
                            break;
                        }
                    }

                    // Send the event(add placeholderimages/2 to index, due to starting with empty images as space occupiers
                    NewMainImage(((Image)((Grid)((UserControl)stackPanel.Children[desiredImageIndex + (placeholderImages / 2)]).Content).Children[0]).Source);

                    scrollTimer.Stop();
                }
            }
        }

        protected void OnParentSizeChanged(object obj, SizeChangedEventArgs args)
        {
            RecalculateSize(args.PreviousSize, args.NewSize);
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
                double distance = 0d;

                if (stackPanel.Orientation == Orientation.Horizontal)
                    distance = stopOffsetPoints[i] - scrollBar.HorizontalOffset;
                else
                    distance = stopOffsetPoints[i] - scrollBar.VerticalOffset;

                if (Math.Abs(distance) < Math.Abs(closestDistance))
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            currentOffsetTarget = stopOffsetPoints[closestIndex];

            scrollTimer.Start();
        }

        private void RecalculateSize(Size oldSize, Size newSize)
        {
            int childrenCount = stackPanel.Children.Count;

            stopOffsetPoints = new double[images.Count];

            double scrollSize;

            if (stackPanel.Orientation == Orientation.Horizontal)
                scrollSize = newSize.Width / numberOfImages;
            else
                scrollSize = newSize.Height / numberOfImages;

            for(int i = 0; i < childrenCount; i++)
            {
                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    ((UserControl)stackPanel.Children[i]).Width = scrollSize;
                    ((UserControl)stackPanel.Children[i]).Height = newSize.Height;
                }
                else {
                    ((UserControl)stackPanel.Children[i]).Height = scrollSize;
                    ((UserControl)stackPanel.Children[i]).Width = newSize.Width;
                }

                if (((UserControl)stackPanel.Children[i]).Content == null) // Images without source are empty placeholders, so no scroll index
                    continue;

                // For the others, populate the stop points
                int newIndex = i - (placeholderImages / 2);
                stopOffsetPoints[newIndex] = newIndex * scrollSize;
            }

            // Keep scroll position with resize
            if (oldSize.Width != 0d && oldSize.Height != 0d)
            {
                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    double oldNewRatio = newSize.Width / oldSize.Width;
                    scrollBar.ScrollToHorizontalOffset(scrollBar.HorizontalOffset * oldNewRatio);
                }
                else {
                    double oldNewRatio = newSize.Height / oldSize.Height;
                    scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset * oldNewRatio);
                }
            }
        }

        public void Populate(List<BitmapImage> images, UserControl sizeItem, Orientation alignment, int numberOfImagesToDisplay, bool displayPage)
        {
            if (populated) // Method can be called twice for some reason, make sure we don't reload
                return;

            populated = true;

            this.images = images;
            sizeItem.SizeChanged += OnParentSizeChanged; // Subscribe to parent size change event, need to adjust image sizes
            stackPanel.Orientation = alignment;
            // Only odd number of images
            numberOfImages = numberOfImagesToDisplay % 2 == 0 ? numberOfImagesToDisplay + 1 : numberOfImagesToDisplay;

            placeholderImages = numberOfImagesToDisplay - 1;

            List<UserControl> emptyImages = new List<UserControl>(placeholderImages);

            for(int i = 0; i < placeholderImages; i++)
            {
                UserControl b = new UserControl();
                b.Padding = new Thickness(5d);
                emptyImages.Add(b);
            }
            
            for(int i = 0; i < placeholderImages / 2; i++)
                stackPanel.Children.Add(emptyImages[i]);

            for(int i = 0; i < images.Count; i++)
            {
                ImageSource s = images[i];

                Image child = new Image();
                child.Source = s;
                child.Stretch = Stretch.Uniform;

                UserControl u = new UserControl();

                Grid g = new Grid();
                u.Content = g;
                
                g.Children.Add(child);

                if (displayPage)
                {
                    Viewbox box = new Viewbox();
                    box.HorizontalAlignment = HorizontalAlignment.Center;
                    box.VerticalAlignment = VerticalAlignment.Center;
                    box.Stretch = Stretch.Uniform;
                    box.Opacity = PAGE_NUMBER_OPACITY;

                    TextBlock text = new TextBlock();
                    text.Text = (i + 1).ToString();
                    box.Child = text;

                    g.Children.Add(box);
                }

                u.Padding = new Thickness(5d);

                stackPanel.Children.Add(u);
            }

            for (int i = placeholderImages / 2; i < placeholderImages; i++)
                stackPanel.Children.Add(emptyImages[i]);

            RecalculateSize(new Size(), new Size(sizeItem.ActualWidth, sizeItem.ActualHeight));
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
