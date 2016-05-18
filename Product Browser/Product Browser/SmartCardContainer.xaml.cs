using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DatabaseModel;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for ImageContainer.xaml
    /// </summary>
    public partial class SmartCardContainer : UserControl
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
        
        // Used by mouse/touch scrolling
        double slideStartOffset;

        #endregion

        #region Properties

        public static Thickness StartingMargins { get; internal set; } = new Thickness(-140, 0, 0, 0);

        #endregion

        #region Events

        public delegate void SmartCardSelectedHandler(object sender, EventArgs args);

        public event SmartCardSelectedHandler SmartCardSelected;

        #endregion

        #region EventHandlers

        #region Slider

        protected void OnSlideMouseDown(object obj, MouseButtonEventArgs e)
        {
            slideStartOffset = e.GetPosition(slideControl).X;
            slideControl.CaptureMouse();
            e.Handled = true;
        }

        protected void OnSlideTouchDown(object obj, TouchEventArgs e)
        {
            slideStartOffset = e.GetTouchPoint(slideControl).Position.X;
            e.TouchDevice.Capture(slideControl);
            e.Handled = true;
        }

        protected void OnSlideMouseMove(object obj, MouseEventArgs e)
        {
            if (slideControl.IsMouseCaptured)
            {
                Point point = e.GetPosition(slideControl);
                double delta = point.X - slideStartOffset;

                Margin = new Thickness(Margin.Left + delta, 0, 0, 0);

                if (Margin.Left < StartingMargins.Left)
                    Margin = StartingMargins;
                else if (Margin.Left > 0)
                    Margin = new Thickness(0);
            }
        }

        protected void OnSlideTouchMove(object obj, TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == slideControl)
            {
                Point point = e.GetTouchPoint(slideControl).Position;
                double delta = point.X - slideStartOffset;

                Margin = new Thickness(Margin.Left + delta, 0, 0, 0);

                if (Margin.Left < StartingMargins.Left)
                    Margin = StartingMargins;
                else if (Margin.Left > 0)
                    Margin = new Thickness(0);
            }
        }

        protected void OnSlideMouseUp(object obj, MouseButtonEventArgs e)
        {
            if (slideControl.IsMouseCaptured)
                slideControl.ReleaseMouseCapture();
        }

        protected void OnSlideTouchUp(object obj, TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == slideControl)
                scrollBar.ReleaseTouchCapture(e.TouchDevice);
        }

        #endregion

        protected void OnSmartCardClicked(object obj, EventArgs args)
        {
            SmartCardSelected((obj as Label).Tag, new EventArgs());
        }

        #endregion

        #region Methods

        public void Populate(List<DatabaseModel.Model.SmartCard> smartcards)
        {
            foreach (var item in smartcards) {

                Label label = new Label();

                label.Tag = item;
                label.Content = item.Name;
                label.Foreground = new SolidColorBrush(new Color() { R = 42, G = 95, B = 111, A = 255 });
                label.BorderBrush = new SolidColorBrush(new Color() { R = 42, G = 95, B = 111, A = 255 });
                label.Background = new SolidColorBrush(Colors.Black);
                label.BorderThickness = new Thickness(1d);

                label.TouchUp += OnSmartCardClicked;
                label.MouseUp += OnSmartCardClicked;

                stackPanel.Children.Add(label);
            }
        }

        #endregion

        public SmartCardContainer()
        {
            InitializeComponent();

            slideControl.MouseDown += OnSlideMouseDown;
            slideControl.TouchDown += OnSlideTouchDown;
            slideControl.MouseMove += OnSlideMouseMove;
            slideControl.TouchMove += OnSlideTouchMove;
            slideControl.MouseUp += OnSlideMouseUp;
            slideControl.TouchUp += OnSlideTouchUp;
        }
    }
}
