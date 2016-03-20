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
using System.Windows.Resources;
using Video;
using System.Drawing.Imaging;
using System.Data.Linq;
using System.Data;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;


namespace ProductBrowser
{
    /// <summary>
    /// A class to hold videos in a container. Inherits from ScatterView.
    /// </summary>
    class VideoContainer : ScatterViewItem
    {
        /// <summary>Object of the type Grid.</summary>
        private Grid videoGrid;
        /// <summary>Object of the type VideoPlayer to hold a vieo control.</summary>
        public VideoPlayer videoControl;
        /// <summary>Object of the type Rectangle.</summary>
        private Border colourFrame;
        /// <summary>Object of the type MainWindow.</summary>
        private MainWindow mainWindow;

        private ABBDataClassesDataContext dc = new ABBDataClassesDataContext();

        private Table<Video> videos;

        private int numberOfVideos;

        private int offeringID;

        private readonly Stopwatch doubleTapStopwatch = new Stopwatch();

        private System.Windows.Point lastTapLocation;

        private Size naturalSize;



        public VideoContainer(int offeringId, int numberOfVideos, TagWindow tagWindow, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.videos = dc.GetTable<Video>();
            this.numberOfVideos = numberOfVideos;
            this.offeringID = offeringId;
            this.videoGrid = new Grid();
            this.videoControl = new VideoPlayer(offeringID);
            this.videoControl.videoPlayer.MediaOpened += video_Opened;
            this.videoGrid.Children.Add(videoControl);
            this.PreviewTouchUp += videoContainer_Clicked;
            //this.ContainerManipulationCompleted += Container_ManipulationCompleted;
            CreateVideoFrame(tagWindow);
            CreateButtons();
            this.Content = videoGrid;
            naturalSize.Width = 100;
        

            for (int i = 0; i < numberOfVideos; i++)
            {
                ElementMenuItem menuButton = new ElementMenuItem();
                try
                {
                    menuButton.Header = DBHelper.GetVideoTitle(offeringId, i);
                    menuButton.Uid = i.ToString();
                    menuButton.FontSize = (int)MainWindow.configurationData.FontSize;
                    menuButton.Click += elementMenuItem_Click;
                    this.videoControl.VideoElementMenu.Items.Add(menuButton);
                }
                catch (Exception ex)
                {
                  
                }
            }
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


        private void videoContainer_Clicked(object sender, TouchEventArgs e)
        {
           
            bool allowDoubleTap = MainWindow.configurationData.DoubleTap.Value;
           
            if (IsDoubleTap(e) && allowDoubleTap)
            {
                PauseVideo();
                videoControl.PlayButton.Visibility = Visibility.Visible;
            }

        }


        private bool IsDoubleTap(TouchEventArgs e)
        {
            System.Windows.Point currentTapPosition = e.GetTouchPoint(this).Position;
            bool tapsAreWithinPage = true;
            foreach (SurfaceButton button in videoGrid.Children.OfType<SurfaceButton>()) // Checks if the touch point targets a button within the documentGrid,
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
        /// This method create buttons.
        /// </summary>
        private void CreateButtons()
        {
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

            videoGrid.Children.Add(btn_close);
        }

        /// <summary>
        /// This method changes the video in the videoplayer when a video in the menu is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void elementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ElementMenuItem menuButtonPressed = e.OriginalSource as ElementMenuItem;
            videoControl.ChangeVideoAtRuntime(DBHelper.GetVideo(offeringID, menuButtonPressed.Header.ToString()));
            BitmapImage myBitmapImage = new BitmapImage(new Uri(@"/Resources/VideoIcon.png", UriKind.RelativeOrAbsolute));
            mainWindow.watchLibrary.list.Add(new FileList(myBitmapImage, "Test kategori", "Test tittel", 2));
        }
        /// <summary>
        /// This method sets the videos height and with.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void video_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double wantedHeight = screenHeight/4; /// (System.Convert.ToInt32(MainWindow.configData.Find(x => x.stringTag.Contains("VideoInitialSize")).stringValue));             //4: Forholdstallet bør flyttes til config
                double compared = videoControl.videoPlayer.NaturalVideoHeight / wantedHeight;
                this.Height = wantedHeight;
                this.Width = videoControl.videoPlayer.NaturalVideoWidth / compared;
            }
            catch { }
        }
        /// <summary>
        /// This method extracts the name from the path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private string ExtractNameFromPath(string path)
        {
            string[] substrings = path.Split('\\');
            return substrings[substrings.Length - 1];
        }
        /// <summary>
        /// This method creates the coloured frame around the picture container.
        /// </summary>
        private void CreateVideoFrame(TagWindow tw)
        {
            colourFrame = new Border();
            colourFrame.BorderThickness = new Thickness((int)MainWindow.configurationData.BoxColorFrameThickness);
            colourFrame.BorderBrush = new SolidColorBrush(tw.GetTagColour());
            videoGrid.Children.Add(colourFrame);
        }
        /// <summary>
        /// This method runs when the close button is clicked. It close the pictureContainer.
        /// </summary>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.videoGrid.Children.Clear();
            mainWindow.ScatterWindow.Items.Remove(this);
            //videoControl.Ms.Listener = null;
            //videoControl.Source = null;
            videoControl = null;

        }

        public void PauseVideo()
        {

            videoControl.playPauseMethod();

        }
    }
}
