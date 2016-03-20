using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SysImages = System.Windows.Controls.Image;
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
using System.Windows.Resources;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Data.Linq;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace ProductBrowser
{   //but sola is cooler
    /// <summary>
    /// A class to hold pictures in a container. Inherits from ScatterView.
    /// </summary>
    class PictureContainer : ScatterViewItem
    {
        /// <summary>Table that holds a pictures index.</summary>
        int pictureIndex = 0;
        /// <summary>Table that holds pictures.</summary>
        string[] pictures;
        /// <summary>Object of the type Grid.</summary>
        private Grid pictureGrid;
        /// <summary>Object of the type Image.</summary>
        private System.Windows.Controls.Image imageControl;
        /// <summary>List with objects of the type Ellipse.</summary>
        private List<Ellipse> listOfIndexCircles;
        /// <summary>List with objects of the type BitmapImage.</summary>
        private List<BitmapImage> listOfPics; // kan kommenteres ut
        /// <summary>Object of the type Canvas.</summary>
        private Canvas circleContainer;
        /// <summary>Object of the type MainWindow.</summary>
        private MainWindow mainWindow;

        private TagWindow tagWindow;
        /// <summary>Object of the type Rectangle.</summary>
        private Border colourFrame;

        /// <summary>
        /// List with ScatterViewItem objects.
        /// </summary>
        private List<ScatterViewItem> listOfSingleImages;

        /// <summary>Object of the type Stopwatch.</summary>
        private readonly Stopwatch doubleTapStopwatch = new Stopwatch();

        private System.Windows.Point lastTapLocation;

        private Size naturalSize;



        ABBDataClassesDataContext dc = new ABBDataClassesDataContext();
        private byte[] imgBinary;
        Table<Image> images;
        //List<Image> imageList;
        private int numberOfImages;
        private int offeringID;
        //private string pictureName;

        public PictureContainer(int offeringId, int numberOfImages, TagWindow tagWindow, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.tagWindow = tagWindow;
            this.images = dc.GetTable<Image>();
            //pictureName = title;
            //this.imageList = images;
            this.numberOfImages = numberOfImages;
            this.offeringID = offeringId;
            this.pictureGrid = new Grid();
            this.imageControl = new System.Windows.Controls.Image();
            this.listOfPics = new List<BitmapImage>();
            listOfSingleImages = new List<ScatterViewItem>();
            Loaded += PictureContainer_Loaded;
            //ConvertImageListToBitmapImageList(images);

            Canvas bgCanvas = new Canvas();
            SolidColorBrush White = new SolidColorBrush(Colors.Black);
            bgCanvas.Background = White;
            bgCanvas.Opacity = 0.5;
            pictureGrid.Children.Add(bgCanvas);

            this.pictureGrid.Children.Add(this.imageControl);
            CreateImageFrame(tagWindow);
            this.PreviewTouchUp += pictureContainer_Clicked;
            this.ContainerManipulationCompleted += Container_ManipulationCompleted;
            setImgByteArray(offeringId, 0);
            DrawIndexCircles();
            CreateButtons();
            imageControl.Source = BlobConverter.ConvertImgBlobtoImage(imgBinary);
            //this.imageControl.Source = listOfPics.First();
            this.Content = this.pictureGrid;
            //mainWindow.ScatterWindow.Items.Add(this);
            this.Visibility = Visibility.Visible;
        }

        private void setImgByteArray(int offeringId, int index)
        {
            try
            {
                //var query = images.Select(x => new { x.ImageID, x.ImageBinary }).Where(x => x.ImageID == offeringId).FirstOrDefault();
                var query =
                    from i in dc.Images
                    where i.OfferingID == offeringId
                    orderby i.ImageID
                    select i;

                int x = 0;
                foreach (Image i in query)
                {
                    if (x == index)
                    {
                        this.imgBinary = i.ImageBinary;
                        break;
                    }
                    else
                    {
                        x++;
                    }
                }


            }
            catch (Exception e)
            {
                MainWindow.warningWindow.Show(e.ToString());
            }
        }


        /// <summary>
        /// This method runs when the pictureContainer is loaded.
        /// </summary>
        private void PictureContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (numberOfImages > 1)
                AddIndexCircles();

            naturalSize = this.RenderSize;
        }


        private void Container_ManipulationCompleted(object sender, RoutedEventArgs e)
        {
            ScatterViewItem _sender = sender as ScatterViewItem;
            const int edgeTolerance = 25;
            this.AllowDrop = false;

            if (_sender.ActualCenter.X < edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 90, naturalSize);
            }
            else if (_sender.ActualCenter.X > System.Windows.SystemParameters.PrimaryScreenWidth - edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 270, naturalSize);

            }
            else if (_sender.ActualCenter.Y < edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 180, naturalSize);

            }
            else if (_sender.ActualCenter.Y > System.Windows.SystemParameters.PrimaryScreenHeight - edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 0, naturalSize);

            }
        }



        private void pictureContainer_Clicked(object sender, TouchEventArgs e)
        {

            bool allowDoubleTap = MainWindow.configurationData.DoubleTap.Value;
           
            if (IsDoubleTap(e) && allowDoubleTap)
            {
                bool itemAlreadyExists = false;
                foreach (ScatterViewItem item in mainWindow.ScatterWindow.Items.OfType<ScatterViewItem>())
                {
                    try
                    {
                        System.Windows.Controls.Image ItemImage = item.Content as System.Windows.Controls.Image;
                        if (ItemImage.Source == imageControl.Source)
                            itemAlreadyExists = true;
                    }
                    catch (Exception ex)
                    {
                        //MainWindow.warningWindow.Show(ex.ToString());
                    }
                }
                if (!itemAlreadyExists)
                {
                    ScatterViewItem singleImage = new ScatterViewItem() { Content = new SysImages() { Source = imageControl.Source } };
                    //listOfSingleImages.Add(singleImage);
                    
                    tagWindow.listOfSingleImages.Add(singleImage);
                    
                    singleImage.ContainerManipulationCompleted += (senderPage, args) =>
                    {
                        if ((singleImage.ActualCenter.X < this.ActualCenter.X + 50) && (singleImage.ActualCenter.X > this.ActualCenter.X - 50) && (singleImage.ActualCenter.Y < this.ActualCenter.Y + 50) && (singleImage.ActualCenter.Y > this.ActualCenter.Y - 50))
                            this.AllowDrop = true;
                    };
                    singleImage.Unloaded += (senderPage, args) =>
                    {
                        this.AllowDrop = false;
                        mainWindow.ScatterWindow.Items.Remove(singleImage);
                    };
                    singleImage.ContainerManipulationCompleted += Container_ManipulationCompleted;
                    
                    mainWindow.ScatterWindow.Items.Add(singleImage);
                    
                    mainWindow.watchLibrary.list.Add(new FileList(BlobConverter.ConvertImgBlobtoImage(imgBinary), "Test kategori", "Test tittel", 2));
                }
                
            }

        }




        /// <summary>
        /// This method is called when a page is tapped to determine whether or not it is a double tap.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsDoubleTap(TouchEventArgs e)
        {
            System.Windows.Point currentTapPosition = e.GetTouchPoint(this).Position;
            bool tapsAreWithinPage = true;
            foreach (SurfaceButton button in pictureGrid.Children.OfType<SurfaceButton>()) // Checks if the touch point targets a button within the documentGrid,
                if (e.TouchDevice.Target == button) { tapsAreWithinPage = false; }          // if so it is certain the taps are not within the bounds of the page itself
            bool tapsAreCloseInDistance = (                                                 // Detremines whether the taps are close in distance or not
                (currentTapPosition.X >= lastTapLocation.X - 15 &&
                currentTapPosition.X <= lastTapLocation.X + 15) &&
                (currentTapPosition.Y >= lastTapLocation.Y - 15 &&
                currentTapPosition.Y <= lastTapLocation.Y + 15));
            lastTapLocation = currentTapPosition;

            TimeSpan elapsed = doubleTapStopwatch.Elapsed;
            doubleTapStopwatch.Restart();
            bool tapsAreCloseInTime = (elapsed != TimeSpan.Zero && elapsed < TimeSpan.FromSeconds(0.7));

            return tapsAreCloseInDistance && tapsAreCloseInTime && tapsAreWithinPage;       // Returns a boolean of whether the last touch qualifies as part of a double tap or not (true or false)
        }

        /// <summary>
        /// This method creates the coloured frame around the picture container.
        /// </summary>
        private void CreateImageFrame(TagWindow tw)
        {
            colourFrame = new Border();
            colourFrame.BorderThickness = new Thickness((int)MainWindow.configurationData.BoxColorFrameThickness);
            colourFrame.BorderBrush = new SolidColorBrush(tw.GetTagColour());
            pictureGrid.Children.Add(colourFrame);
            
        }
        /// <summary>
        /// This method draws the images
        /// </summary>
        /*private void DrawImages()
        {
            for (int i = 0; i < pictures.Length; i++)
            {
                try
                {
                    BitmapImage myBitmapImage = new BitmapImage(new Uri(pictures[i]));
                    listOfPics.Add(myBitmapImage);
                }
                catch (Exception ex)
                {
                    pictures[i] = "Resources/PictureNotFound.png";
                    BitmapImage myBitmapImage = new BitmapImage(new Uri(pictures[i], UriKind.Relative));
                    listOfPics.Add(myBitmapImage);
                }
            }
        }*/

        /// <summary>
        /// This method create buttons.
        /// </summary>
        private void CreateButtons()
        {
            //if (pictures.Length > 1)
            if (numberOfImages > 1)
            {
                StreamResourceInfo nextStreamInfo = Application.GetResourceStream(new Uri("Resources/btnTransNext.png", UriKind.Relative));
                BitmapFrame nextButtonImage = BitmapFrame.Create(nextStreamInfo.Stream);
                ImageBrush nextImgBrush = new ImageBrush();
                nextImgBrush.ImageSource = nextButtonImage;

                SurfaceButton btn_next = new SurfaceButton();
                btn_next.Background = nextImgBrush;
                btn_next.Click += btn_next_Click;

                btn_next.VerticalAlignment = VerticalAlignment.Bottom;
                btn_next.HorizontalAlignment = HorizontalAlignment.Right;
                btn_next.Margin = new Thickness(0, 0, 0, -40);

                StreamResourceInfo prevStreamInfo = Application.GetResourceStream(new Uri("Resources/btnTransPrevious.png", UriKind.Relative));
                BitmapFrame prevButtonImage = BitmapFrame.Create(prevStreamInfo.Stream);
                ImageBrush prevImgBrush = new ImageBrush();
                prevImgBrush.ImageSource = prevButtonImage;

                SurfaceButton btn_prev = new SurfaceButton();
                btn_prev.Background = prevImgBrush;
                btn_prev.Click += btn_prev_Click;

                btn_prev.VerticalAlignment = VerticalAlignment.Bottom;
                btn_prev.HorizontalAlignment = HorizontalAlignment.Left;
                btn_prev.Margin = new Thickness(0, 0, 0, -40);

                pictureGrid.Children.Add(btn_next);
                pictureGrid.Children.Add(btn_prev);
            }

            StreamResourceInfo closeStreamInfo = Application.GetResourceStream(new Uri("Resources/btnX.png", UriKind.Relative));
            BitmapFrame closeButtonImage = BitmapFrame.Create(closeStreamInfo.Stream);
            ImageBrush closeImgBrush = new ImageBrush();
            closeImgBrush.ImageSource = closeButtonImage;

            SurfaceButton btn_close = new SurfaceButton();
            btn_close.Background = closeImgBrush;
            btn_close.Click += btn_close_Click;

            btn_close.VerticalAlignment = VerticalAlignment.Top;
            btn_close.HorizontalAlignment = HorizontalAlignment.Right;
            btn_close.MinWidth = (int)MainWindow.configurationData.CloseButtonSize;
            btn_close.MinHeight = (int)MainWindow.configurationData.CloseButtonSize;
            btn_close.Margin = new Thickness(0, -20, -20, 0);

            pictureGrid.Children.Add(btn_close);
        }

        /// <summary>
        /// This method draws index circles.
        /// </summary>
        private void DrawIndexCircles()
        {
            //if (pictures.Length > 1)
            if (numberOfImages > 1)
            {
                circleContainer = new Canvas();
                listOfIndexCircles = new List<Ellipse>();
                int spacing = 0;

                //for (int i = 0; i < pictures.Length; i++)
                for (int i = 0; i < numberOfImages; i++)
                {
                    Ellipse indexCircle = new Ellipse();
                    indexCircle.Width = 10;
                    indexCircle.Height = 10;
                    indexCircle.StrokeThickness = 1;
                    indexCircle.Fill = new SolidColorBrush(Colors.Black);
                    indexCircle.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
                    circleContainer.Children.Add(indexCircle);
                    Canvas.SetLeft(indexCircle, spacing);
                    spacing = spacing + 15;
                    listOfIndexCircles.Add(indexCircle);
                }

                listOfIndexCircles[0].Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
                circleContainer.VerticalAlignment = VerticalAlignment.Bottom;
                circleContainer.HorizontalAlignment = HorizontalAlignment.Center;
                circleContainer.Margin = new Thickness(5, 5, 5, -5);

                pictureGrid.Children.Add(circleContainer);
                listOfIndexCircles[0].Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
            }
        }
        /// <summary>
        /// This method is called when the picture container is loaded. Index cricles are drawn if any exists.
        /// </summary>
        private void AddIndexCircles()
        {
            foreach (Ellipse indexCircle in listOfIndexCircles)
            {
                Canvas.SetLeft(indexCircle, -(listOfIndexCircles.Count * (25 / 2) - listOfIndexCircles.IndexOf(indexCircle) * 25) / 2);
            }
        }

        /// <summary>
        /// This method is called when the next or previous picture buttons are clicked. It shifts the index of the picture.
        /// </summary>
        private void ChangePictureToIndex(int index)
        {
            //if (index >= pictures.Length)
            if (index >= numberOfImages)
            {
                pictureIndex = 0;
                index = 0;
            }

            //imageControl.Source = listOfPics[index];

            setImgByteArray(offeringID, index);
            this.imageControl.Source = BlobConverter.ConvertImgBlobtoImage(imgBinary);
            
            //if (pictureIndex != 0)
            //    listOfEllipses[index - 1].Fill = new SolidColorBrush(Colors.Black);
            //else
            //    listOfEllipses[pictures.Length - 1].Fill = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// This method runs when the next picture button is clicked.
        /// </summary>
        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            ChangePictureToIndex(++pictureIndex);
            listOfIndexCircles[pictureIndex].Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
            if (pictureIndex == 0)
            {
                //listOfIndexCircles[pictures.Length - 1].Fill = new SolidColorBrush(Colors.Black);
                listOfIndexCircles[numberOfImages - 1].Fill = new SolidColorBrush(Colors.Black);
            }
            else
                listOfIndexCircles[pictureIndex - 1].Fill = new SolidColorBrush(Colors.Black);
        }
        /// <summary>
        /// This method runs when the previous picture button is clicked.
        /// </summary>
        private void btn_prev_Click(object sender, RoutedEventArgs e)
        {
            if (pictureIndex == 0)
            {
                //pictureIndex = pictures.Length;
                pictureIndex = numberOfImages;
            }
            ChangePictureToIndex(--pictureIndex);

            listOfIndexCircles[pictureIndex].Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
            //if (pictureIndex == pictures.Length - 1)
            if (pictureIndex == numberOfImages - 1)
            {
                listOfIndexCircles[0].Fill = new SolidColorBrush(Colors.Black);
            }
            else
                listOfIndexCircles[pictureIndex + 1].Fill = new SolidColorBrush(Colors.Black);
        }
        /// <summary>
        /// This method runs when the close button is clicked. It close the pictureContainer.
        /// </summary>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //ClearSingleImages();
            mainWindow.ScatterWindow.Items.Remove(this);

        }

       /* public void ClearSingleImages()
        {
            foreach (ScatterViewItem singlePage in listOfSingleImages)
                mainWindow.ScatterWindow.Items.Remove(singlePage);
        }*/
    }
}
