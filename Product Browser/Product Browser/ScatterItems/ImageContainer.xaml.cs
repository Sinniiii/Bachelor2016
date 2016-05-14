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

        private const double
            SCROLL_SPEED = 10d,
            PAGE_NUMBER_OPACITY = 0.15d,
            INACTIVE_OPACITY = 0.4d;

        private double numberOfImages = 3d;
        private int placeholderImages = 2;

        UserControl selectedImage;

        List<BitmapImage> images;
        
        // For automatically scrolling to closest item point
        double[] stopOffsetPoints;
        double currentOffsetTarget;
        DispatcherTimer scrollTimer;
        
        // Used by mouse/touch scrolling
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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

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

            if(selectedImage != null)
            {
                selectedImage.Opacity = INACTIVE_OPACITY;
                selectedImage = null;
            }
            
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);

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

            if (selectedImage != null)
            {
                selectedImage.Opacity = INACTIVE_OPACITY;
                selectedImage = null;
            }

            e.TouchDevice.Capture(this);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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

        protected override void OnTouchMove(TouchEventArgs e)
        {
            base.OnTouchMove(e);

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

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            StartAutoScroll();
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);

            if (e.TouchDevice.Captured == this)
                ReleaseTouchCapture(e.TouchDevice);

            StartAutoScroll();
        }

        protected void OnSizeChanged(object obj, SizeChangedEventArgs args)
        {
            RecalculateSize(args.PreviousSize, args.NewSize);
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

            int desiredImageIndex = -1;

            if (stackPanel.Orientation == Orientation.Horizontal)
            {
                scrollBar.ScrollToHorizontalOffset(scrollBar.HorizontalOffset + speed);

                // Check if we arrived
                if (scrollBar.HorizontalOffset > currentOffsetTarget - 0.1d && scrollBar.HorizontalOffset < currentOffsetTarget + 0.1d)
                {
                    // Find the correct image
                    for (int i = 0; i < stopOffsetPoints.Length; i++)
                    {
                        if (stopOffsetPoints[i] > scrollBar.HorizontalOffset - 0.1d && stopOffsetPoints[i] < scrollBar.HorizontalOffset + 0.1d)
                        {
                            desiredImageIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset + speed);

                // Check if we arrived
                if (scrollBar.VerticalOffset > currentOffsetTarget - 0.1d && scrollBar.VerticalOffset < currentOffsetTarget + 0.1d)
                {
                    // Find the correct image
                    for (int i = 0; i < stopOffsetPoints.Length; i++)
                    {
                        if (stopOffsetPoints[i] > scrollBar.VerticalOffset - 0.1d && stopOffsetPoints[i] < scrollBar.VerticalOffset + 0.1d)
                        {
                            desiredImageIndex = i;
                            break;
                        }
                    }
                }
            }
            
            if(desiredImageIndex != -1) // We have arrived, check the new image
            {
                // Send the event(add placeholderimages/2 to index, due to starting with empty images as space occupiers
                NewMainImage(((Image)((Grid)((UserControl)stackPanel.Children[desiredImageIndex + (placeholderImages / 2)]).Content).Children[0]).Source);
                // Add highlight
                selectedImage = stackPanel.Children[desiredImageIndex + (placeholderImages / 2)] as UserControl;
                selectedImage.Opacity = 1d;

                scrollTimer.Stop();
            }
        }   

        #endregion

        #region Methods

        protected void StartAutoScroll()
        {
            // Calculate closest stop point
            double closestDistance = 999999999d;
            int closestIndex = -1;

            for (int i = 0; i < stopOffsetPoints.Length; i++)
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
            if (images == null) // Can be called before we've received images, on initial placement, so jump out early
                return;

            int childrenCount = stackPanel.Children.Count;

            stopOffsetPoints = new double[images.Count];

            double scrollSize;

            if (stackPanel.Orientation == Orientation.Horizontal)
                scrollSize = newSize.Width / numberOfImages;
            else
                scrollSize = newSize.Height / numberOfImages;

            double offset = numberOfImages % 2 == 0 ? 0.5 * scrollSize : scrollSize;

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
                stopOffsetPoints[newIndex] = newIndex * scrollSize + offset;
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

        public void Populate(List<BitmapImage> images, Orientation alignment, int numberOfImagesToDisplay, bool displayNumber)
        {
            this.images = images;
            stackPanel.Orientation = alignment;
            this.numberOfImages = numberOfImagesToDisplay;

            if (stackPanel.Children.Count > 0) // Re-initialization ?
                stackPanel.Children.Clear();
            
            // Need even number of placeholders
            placeholderImages = numberOfImagesToDisplay % 2 == 0 ? numberOfImagesToDisplay : numberOfImagesToDisplay + 1;

            // Create placeholder images for beginning and end so we can center first and last images
            List<UserControl> placeholders = new List<UserControl>(placeholderImages);
            for(int i = 0; i < placeholderImages; i++)
            {
                UserControl b = new UserControl();
                placeholders.Add(b);
            }
            
            // Add half the placeholders
            for(int i = 0; i < placeholderImages / 2; i++)
                stackPanel.Children.Add(placeholders[i]);

            // Add all images
            for(int i = 0; i < images.Count; i++)
            {
                Image child = new Image();
                child.Source = images[i];
                child.Stretch = Stretch.Fill;
                
                UserControl u = new UserControl();

                u.BorderBrush = new SolidColorBrush(new Color() { R = 42, G = 95, B = 111, A = 255});
                u.BorderThickness = new Thickness(1d);

                Grid g = new Grid();
                u.Content = g;
                
                g.Children.Add(child);

                if (displayNumber)
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

                if (i == 0)
                    selectedImage = u;
                else
                    u.Opacity = INACTIVE_OPACITY;

                stackPanel.Children.Add(u);
            }

            // Add remanining images
            for (int i = placeholderImages / 2; i < placeholderImages; i++)
                stackPanel.Children.Add(placeholders[i]);

            // Now that we have all elements, recalculate container size
            RecalculateSize(new Size(), scrollBar.RenderSize);

            // And scroll to first element
            if(alignment == Orientation.Vertical)
                scrollBar.ScrollToVerticalOffset(stopOffsetPoints[0]);
            else
                scrollBar.ScrollToHorizontalOffset(stopOffsetPoints[0]);
        }

        #endregion

        public ImageContainer()
        {
            InitializeComponent();

            scrollTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            scrollTimer.Tick += OnScrollTimer;

            // Need to know when the scrollbar control changes size, so we can reshape elements to fit
            scrollBar.SizeChanged += OnSizeChanged;
        }
    }
}
