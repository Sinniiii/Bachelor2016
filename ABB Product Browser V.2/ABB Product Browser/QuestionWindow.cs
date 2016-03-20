using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Threading;
using System.ComponentModel;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for WarningWindow.xaml
    /// </summary>
    public partial class QuestionWindow: Grid
    {
        private MainWindow mainWindow;

        private Border frame;
        private Label lbl_message;

        //public string sourceFileName;
        //public string destinationFileName;
        private SurfaceButton btn_Yes;
        private SurfaceButton btn_No;

        /// <summary>
        /// Constructor initializes graphics and functionality of the warning window.
        /// </summary>
        /// <param name="mainWindow"></param>
        public QuestionWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            CreateWindow();
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            Loaded += QuestionWindow_Loaded;
        }

        private void QuestionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btn_Yes.Margin = new Thickness(-90, lbl_message.ActualHeight + 20, 0, 10);
            btn_No.Margin = new Thickness(90, lbl_message.ActualHeight + 20, 0, 10);
        }

        /// <summary>
        /// Method that creates the objects in the warning window.
        /// </summary>
        private void CreateWindow()
        {
            this.Background = Brushes.Black;
            frame = new Border();
            frame.BorderThickness = new Thickness(5);
            frame.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
            lbl_message = new Label() { Content = "Message", Margin = new Thickness(12, 5, 0, 0), FontSize = 14, MaxHeight = 200, MaxWidth = 240, Width = 280, HorizontalAlignment = HorizontalAlignment.Left };
            btn_Yes = new SurfaceButton()
            {
                Content = "Yes",
                Height = 38,
                Width = 76,
                MinHeight = 20,
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF)),
                Background = new LinearGradientBrush(
                    new GradientStopCollection {
                            new GradientStop(Color.FromArgb(0xFF,0x3C,0x3C,0x3C), 0.0), 
                            new GradientStop(Color.FromArgb(0xFF,0x02,0x02,0x02), 1.0)
                        },
                new System.Windows.Point(0.5, 0),
                new System.Windows.Point(0.5, 1))
            };
            btn_Yes.Click += btn_Yes_Click;

            btn_No = new SurfaceButton()
            {
                Content = "No",
                Height = 38,
                Width = 76,
                MinHeight = 20,
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF)),
                Background = new LinearGradientBrush(
                    new GradientStopCollection {
                            new GradientStop(Color.FromArgb(0xFF,0x3C,0x3C,0x3C), 0.0), 
                            new GradientStop(Color.FromArgb(0xFF,0x02,0x02,0x02), 1.0)
                        },
                new System.Windows.Point(0.5, 0),
                new System.Windows.Point(0.5, 1))
            };
            btn_No.Click += btn_No_Click;

            this.Children.Add(btn_Yes);
            this.Children.Add(btn_No);
            this.Children.Add(frame);
            this.Children.Add(lbl_message);
        }

        /// <summary>
        /// Method shows the message sendt to it in the message label.
        /// </summary>
        /// <param name="message"></param>
        public void Show(string message)
        {
            mainWindow.RefreshContent();
            mainWindow.mainGrid.Children.Add(this);
            mainWindow.questionWindow = this;
            lbl_message.Content = message;
        }

        /// <summary>
        /// Method that runs when the OK button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// /// <param name="e"></param>
        private void btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.reprogramTags.answerQuestion(this, MessageBoxResult.Yes);
            mainWindow.mainGrid.Children.Remove(this);
            mainWindow.mainGrid.Children.Remove(mainWindow.questionWindow);
            mainWindow.mainGrid.AllowDrop = true;
            mainWindow.RefreshContent();
            mainWindow.ToolBarGrid.Visibility = Visibility.Visible;
            mainWindow.BlurEffect.Radius = 0;
            mainWindow.BlurEffectBack.Radius = 0;
            mainWindow.ScatterWindow.Visibility = Visibility.Visible;
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.reprogramTags.answerQuestion(this, MessageBoxResult.No);
            mainWindow.mainGrid.Children.Remove(this);
            mainWindow.mainGrid.Children.Remove(mainWindow.questionWindow);
            mainWindow.RefreshContent();
        }
    }
}