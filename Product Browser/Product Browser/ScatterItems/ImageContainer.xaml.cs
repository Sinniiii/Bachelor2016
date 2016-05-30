using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for ImageContainer.xaml
    /// </summary>
    public partial class ImageContainer : UserControl, INotifyPropertyChanged
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
            PAGE_NUMBER_OPACITY = 0.15d,
            INACTIVE_OPACITY = 0.7d,
            USER_SELECTED_IMAGE_ERROR_MARGIN = 3d,
            SCROLL_MULTIPLIER = 2d;

        private double numberOfImages = 3d;
        private int placeholderImages = 2;

        UserControl
            currentlySelectedImage,
            userSelectedImage;

        bool scrollingToUserSelected = false;
        
        // For automatically scrolling to closest item point
        double[] stopOffsetPoints;
        double currentOffsetTarget;
        DispatcherTimer scrollTimer;

        // Used by mouse/touch scrolling
        double scrollStartPoint,
                scrollStartOffset,
                scrollSpeed = 10d;

        #endregion

        #region Properties

        private Color colorTheme;
        public Color ColorTheme
        {
            get { return colorTheme; }
            set { colorTheme = value; ColorThemeBrush = new SolidColorBrush(new Color() { R = value.R, G = value.G, B = value.B, A = 235 }); NotifyPropertyChanged(); }
        }

        private SolidColorBrush colorThemeBrush;
        public SolidColorBrush ColorThemeBrush
        {
            get { return colorThemeBrush; }
            set { this.colorThemeBrush = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Events

        public delegate void NewMainImageHandler(int index);
        public event NewMainImageHandler NewMainImage;

        #endregion

        #region EventHandlers

        protected void OnScrollBarMouseDown(object obj, MouseButtonEventArgs e)
        {
            if (stackPanel.Orientation == Orientation.Horizontal)
            {
                scrollStartPoint = e.GetPosition(scrollBar).X;
                scrollStartOffset = scrollBar.HorizontalOffset;
            }
            else
            {
                scrollStartPoint = e.GetPosition(scrollBar).Y;
                scrollStartOffset = scrollBar.VerticalOffset;
            }

            if (!(e.MouseDevice.DirectlyOver is Image))
                scrollingToUserSelected = false;

            scrollBar.CaptureMouse();
            e.Handled = true;
        }

        protected void OnScrollBarTouchDown(object obj, TouchEventArgs e)
        {
            if (stackPanel.Orientation == Orientation.Horizontal)
            {
                scrollStartPoint = e.GetTouchPoint(scrollBar).Position.X;
                scrollStartOffset = scrollBar.HorizontalOffset;
            }
            else
            {
                scrollStartPoint = e.GetTouchPoint(scrollBar).Position.Y;
                scrollStartOffset = scrollBar.VerticalOffset;
            }

            if (!(e.TouchDevice.DirectlyOver is Image))
                scrollingToUserSelected = false;

            e.TouchDevice.Capture(scrollBar);
            e.Handled = true;
        }

        protected void OnScrollBarMouseMove(object obj, MouseEventArgs e)
        {
            if (scrollBar.IsMouseCaptured)
            {
                Point point = e.GetPosition(scrollBar);
                
                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    double delta = point.X - scrollStartPoint;

                    if (Math.Abs(delta) > USER_SELECTED_IMAGE_ERROR_MARGIN)
                        scrollingToUserSelected = false;

                    delta *= SCROLL_MULTIPLIER;

                    scrollBar.ScrollToHorizontalOffset(-delta + scrollStartOffset);
                }
                else
                {
                    double delta = point.Y - scrollStartPoint;

                    if (Math.Abs(delta) > USER_SELECTED_IMAGE_ERROR_MARGIN)
                        scrollingToUserSelected = false;

                    delta *= SCROLL_MULTIPLIER;

                    scrollBar.ScrollToVerticalOffset(-delta + scrollStartOffset);
                }
            }
        }

        protected void OnScrollBarTouchMove(object obj, TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == scrollBar)
            {
                Point point = e.GetTouchPoint(scrollBar).Position;

                if (stackPanel.Orientation == Orientation.Horizontal)
                {
                    double delta = point.X - scrollStartPoint;

                    if (Math.Abs(delta) > USER_SELECTED_IMAGE_ERROR_MARGIN)
                        scrollingToUserSelected = false;

                    delta *= SCROLL_MULTIPLIER;

                    scrollBar.ScrollToHorizontalOffset(-delta + scrollStartOffset);
                }
                else
                {
                    double delta = point.Y - scrollStartPoint;

                    if (Math.Abs(delta) > USER_SELECTED_IMAGE_ERROR_MARGIN)
                        scrollingToUserSelected = false;

                    delta *= SCROLL_MULTIPLIER;

                    scrollBar.ScrollToVerticalOffset(-delta + scrollStartOffset);
                }
            }
        }

        protected void OnScrollBarMouseUp(object obj, MouseButtonEventArgs e)
        {
            if (scrollBar.IsMouseCaptured)
                scrollBar.ReleaseMouseCapture();

            if (scrollingToUserSelected)
                StartAutoScroll((int)userSelectedImage.Tag);
            else
                StartAutoScroll(-1);
        }

        protected void OnScrollBarTouchUp(object obj, TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == scrollBar)
                scrollBar.ReleaseTouchCapture(e.TouchDevice);

            if (scrollingToUserSelected)
                StartAutoScroll((int)userSelectedImage.Tag);
            else
                StartAutoScroll(-1);
        }

        protected void OnScrollBarSizeChanged(object obj, SizeChangedEventArgs args)
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

            double speed = delta > 0 ? scrollSpeed : -scrollSpeed;

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
                // Send the event
                NewMainImage(desiredImageIndex);

                // Remove old highlight, if any
                if (currentlySelectedImage != null)
                    currentlySelectedImage.Opacity = INACTIVE_OPACITY;

                // Add highlight
                currentlySelectedImage = stackPanel.Children[desiredImageIndex + (placeholderImages / 2)] as UserControl;
                currentlySelectedImage.Opacity = 1d;

                scrollTimer.Stop();
            }
        }   

        protected void OnUserImageSelected(object sender, EventArgs args)
        {
            userSelectedImage = sender as UserControl;
            scrollingToUserSelected = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Auto-scrolls to desired index. An input outside the valid container range will auto scroll to closest index.
        /// </summary>
        /// <param name="desiredIndex"></param>
        protected void StartAutoScroll(int desiredIndex)
        {
            // Calculate closest stop point
            double closestDistance = 999999999d;
            int closestIndex = -1;

            if (desiredIndex >= 0 && desiredIndex < stopOffsetPoints.Length)
                closestIndex = desiredIndex;
            else
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

        private void toFirst_Click(object sender, RoutedEventArgs e)
        {
            StartAutoScroll(0);

            e.Handled = true;
        }

        private void toLast_Click(object sender, RoutedEventArgs e)
        {
            StartAutoScroll(stopOffsetPoints.Length - 1);

            e.Handled = true;
        }

        private void RecalculateSize(Size oldSize, Size newSize)
        {
            if (stopOffsetPoints == null) // Can be called before we've received images, on initial placement, so jump out early
                return;

            int childrenCount = stackPanel.Children.Count;

            double scrollSize;

            if (stackPanel.Orientation == Orientation.Horizontal)
                scrollSize = newSize.Width / numberOfImages;
            else
                scrollSize = newSize.Height / numberOfImages;

            // Update scroll speed
            scrollSpeed = scrollSize / 3.5;

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
                    // If a scroll is currently in progress, make sure the offset target is changed too
                    currentOffsetTarget *= oldNewRatio;
                }
                else {
                    double oldNewRatio = newSize.Height / oldSize.Height;
                    scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset * oldNewRatio);
                    // If a scroll is currently in progress, make sure the offset target is changed too
                    currentOffsetTarget *= oldNewRatio;
                }
            }
        }

        public void Populate(List<BitmapImage> images, Orientation alignment, int numberOfImagesToDisplay, bool displayNavButtons, bool displayNumber)
        {
            stackPanel.Orientation = alignment;
            this.numberOfImages = numberOfImagesToDisplay;

            if (stackPanel.Children.Count > 0) // Re-initialization ?
                stackPanel.Children.Clear();

            stopOffsetPoints = new double[images.Count];

            // Need even number of placeholders
            placeholderImages = numberOfImagesToDisplay % 2 == 0 ? numberOfImagesToDisplay : numberOfImagesToDisplay + 1;

            // Create placeholder images for beginning and end so we can center first and last images
            List<UserControl> placeholders = new List<UserControl>(placeholderImages);
            for(int i = 0; i < placeholderImages; i++)
            {
                UserControl b = new UserControl();
                b.IsHitTestVisible = false;
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

                Binding binding = new Binding("ColorThemeBrush");
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ImageContainer), 1);
                u.SetBinding(UserControl.BorderBrushProperty, binding);

                u.BorderThickness = new Thickness(1d);
                u.Tag = i;

                u.PreviewTouchDown += OnUserImageSelected;
                u.PreviewMouseDown += OnUserImageSelected;

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
                    box.IsHitTestVisible = false;

                    TextBlock text = new TextBlock();
                    text.Text = (i + 1).ToString();
                    box.Child = text;
                    text.IsHitTestVisible = false;

                    g.Children.Add(box);
                }

                if (i == 0)
                    currentlySelectedImage = u;
                else
                    u.Opacity = INACTIVE_OPACITY;

                stackPanel.Children.Add(u);
            }

            // Add remanining images
            for (int i = placeholderImages / 2; i < placeholderImages; i++)
                stackPanel.Children.Add(placeholders[i]);

            
            if (!displayNavButtons)
            {
                leftButton.Visibility = rightButton.Visibility = upButton.Visibility = downButton.Visibility = Visibility.Hidden;
                col0.Width = col2.Width = row0.Height = row2.Height = new GridLength(0);
            }
            else if (alignment == Orientation.Vertical)
            {
                leftButton.Visibility = rightButton.Visibility = Visibility.Hidden;
                col0.Width = col2.Width = new GridLength(0);
            }
            else {
                upButton.Visibility = downButton.Visibility = Visibility.Hidden;
                row0.Height = row2.Height = new GridLength(0);
            }

            // Now that we have all elements, recalculate container size
            RecalculateSize(new Size(), scrollBar.RenderSize);

            // Scroll to first element
            StartAutoScroll(0);
        }

        #endregion

        public ImageContainer()
        {
            InitializeComponent();

            scrollTimer = new DispatcherTimer(DispatcherPriority.Render, this.Dispatcher);
            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            scrollTimer.Tick += OnScrollTimer;

            scrollBar.MouseDown += OnScrollBarMouseDown;
            scrollBar.TouchDown += OnScrollBarTouchDown;
            scrollBar.MouseMove += OnScrollBarMouseMove;
            scrollBar.TouchMove += OnScrollBarTouchMove;
            scrollBar.MouseUp += OnScrollBarMouseUp;
            scrollBar.TouchUp += OnScrollBarTouchUp;

            // Need to know when the scrollbar control changes size, so we can reshape elements to fit
            scrollBar.SizeChanged += OnScrollBarSizeChanged;
        }
    }
}
