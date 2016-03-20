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
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace ProductBrowser
{
    /// <summary>
    /// A class to replace the tag. Inherits from ScatterView.
    /// </summary>
    public class VirtualCard : Grid
    {
        /// <summary>Object of the type Rectangle.</summary>
        private Rectangle virtualCardDrawing;
        /// <summary>Object of the type TagWindow.</summary>
        private TagWindow associatedTagWindow;
        /// <summary>Object of the type MainWindow.</summary>
        private MainWindow associatedMainWindow;

        /// <summary>Object of the type Label.</summary>
        private Label lbl_Name_Bottom;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_Name_Top;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_Name_Right;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_Name_Left;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_TagValue;

        /// <summary>
        /// Constructor that contains an object of the type TagWindow and of the type MainWindow.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="mainWin"></param>
        public VirtualCard(TagWindow t, MainWindow mainWin)
        {
            associatedTagWindow = t;
            associatedMainWindow = mainWin;

            virtualCardDrawing = new Rectangle();
            lbl_Name_Bottom = new Label();
            lbl_Name_Top = new Label();
            lbl_Name_Left = new Label();
            lbl_Name_Right = new Label();
            lbl_TagValue = new Label();

            Loaded += VirtualCard_Loaded;
        }
        /// <summary>
        /// Method that runs when the virtual card is loaded to draw its content.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="mainWin"></param>
        private void VirtualCard_Loaded(object sender, RoutedEventArgs e)
        {
            const int fontSize = 10;
            
            virtualCardDrawing.Width = 190;
            virtualCardDrawing.Height = 120;
            virtualCardDrawing.Fill = new SolidColorBrush(Colors.Black);
            virtualCardDrawing.Stroke = new SolidColorBrush(associatedTagWindow.GetTagColour());
            virtualCardDrawing.StrokeThickness = (int)MainWindow.configurationData.CardColorFrameThickness;

            string tagValueHex = associatedTagWindow.VisualizedTag.Value.ToString("X");
            if (tagValueHex.Length < 2) { tagValueHex = "0" + tagValueHex; }
            lbl_TagValue.Content = tagValueHex;
            lbl_TagValue.FontSize = fontSize;
            lbl_TagValue.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x25, 0x25));
            lbl_TagValue.HorizontalAlignment = HorizontalAlignment.Center;
            lbl_TagValue.VerticalAlignment = VerticalAlignment.Center;
            lbl_TagValue.Margin = new Thickness(0, 10, 0, 0);

            lbl_Name_Bottom.Content = associatedTagWindow.GetProductName();
            lbl_Name_Bottom.FontSize = fontSize;
            lbl_Name_Bottom.HorizontalAlignment = HorizontalAlignment.Center;
            lbl_Name_Bottom.VerticalAlignment = VerticalAlignment.Bottom;
            lbl_Name_Bottom.Margin = new Thickness(0, 0, 0, -15);

            lbl_Name_Top.Content = associatedTagWindow.GetProductName();
            lbl_Name_Top.FontSize = fontSize;
            lbl_Name_Top.LayoutTransform = new RotateTransform(180);
            lbl_Name_Top.HorizontalAlignment = HorizontalAlignment.Center;
            lbl_Name_Top.VerticalAlignment = VerticalAlignment.Top;
            lbl_Name_Top.Margin = new Thickness(0, -15, 0, 0);

            lbl_Name_Right.Content = associatedTagWindow.GetProductName();
            lbl_Name_Right.FontSize = fontSize;
            lbl_Name_Right.LayoutTransform = new RotateTransform(-90);
            lbl_Name_Right.HorizontalAlignment = HorizontalAlignment.Right;
            lbl_Name_Right.VerticalAlignment = VerticalAlignment.Center;
            lbl_Name_Right.Margin = new Thickness(0, 0, -15, 0);

            lbl_Name_Left.Content = associatedTagWindow.GetProductName();
            lbl_Name_Left.FontSize = fontSize;
            lbl_Name_Left.LayoutTransform = new RotateTransform(90);
            lbl_Name_Left.HorizontalAlignment = HorizontalAlignment.Left;
            lbl_Name_Left.VerticalAlignment = VerticalAlignment.Center;
            lbl_Name_Left.Margin = new Thickness(-15, 0, 0, 0);

            this.Width = virtualCardDrawing.Width;
            this.Height = virtualCardDrawing.Height;
            try
            {
                this.Children.Add(virtualCardDrawing);
                this.Children.Add(lbl_Name_Bottom);
                this.Children.Add(lbl_Name_Top);
                this.Children.Add(lbl_Name_Right);
                this.Children.Add(lbl_Name_Left);
                this.Children.Add(lbl_TagValue);
            }
            catch (Exception ex)
            {

            }
            
        }

        /// <summary>
        /// Method that gets the tagWindow
        /// </summary>
        /// <returns></returns>
        public TagWindow GetTagWindow()
        {
            return this.associatedTagWindow;
        }
    }
}
