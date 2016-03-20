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
using System.Windows.Threading;
using System.Timers;
using ProductBrowser;
using Microsoft.Surface.Presentation.Controls;
using System.Net;
using System.ComponentModel;
using System.Data.Linq;
using System.Data;
using System.IO;
using System.Windows.Media.Animation;
using SysImages = System.Windows.Controls.Image;

namespace Video
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl, INotifyPropertyChanged
    {
        /// <summary>private variable for the duration of a video. </summary>
        private double totalVideoDuration = 1000;
        /// <summary>Object of the type timer. </summary>
        public Timer _playTimer;

        private byte[] videoSource;

        private Uri sourceUri;

        private MediaServer mediaServer;

        private int offeringID;

        private bool isPlaying = false;

        private System.Windows.Threading.DispatcherTimer t;

        #region Public Properties

        public Uri Source
        {
            get { return sourceUri; }
            set
            {
                sourceUri = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Source"));
            }
        }

        public byte[] VideoSource
        {
            get { return videoSource; }
            set
            {
                videoSource = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("VideoSource"));
            }
        }

        public MediaServer Ms
        {
            get { return mediaServer; }
            set
            {
                mediaServer = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Ms"));
            }
        }

        /// <summary>Property for setting and getting the video progress. </summary>
        public double CurrentVideoProgress
        {
            get { return (double)GetValue(_currentVideoProgress); }
            set { SetValue(_currentVideoProgress, value); }
        }

        /// <summary>Property for... the boolic member _currentVideoProgress.</summary>
        public static readonly DependencyProperty _currentVideoProgress = DependencyProperty.Register("CurrentVideoProgress", typeof(double), typeof(VideoPlayer), new FrameworkPropertyMetadata((double)0));

        /// <summary>Property for setting and getting a boolic variable when the video is playing.</summary>
        public bool VideoIsPlaying
        {
            get { return (bool)GetValue(_videoIsPlaying); }
            set { SetValue(_videoIsPlaying, value); }
        }

        /// <summary> Property for... the boolic member _videoisplaying.</summary>
        public static readonly DependencyProperty _videoIsPlaying = DependencyProperty.Register("VideoIsPlaying", typeof(bool), typeof(VideoPlayer), new FrameworkPropertyMetadata(false));

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary
        public VideoPlayer(int offeringID)
        {
            InitializeComponent();
            string title = DBHelper.GetVideoTitle(offeringID, 0);
            this.VideoSource = DBHelper.GetVideo(offeringID, title);
            this.Source = null;
            GlobalUtilities.incrementIndex();
            this.Source = new Uri("http://localhost:8080/" + GlobalUtilities.LocalhostIndex + "/");

            Loaded += new RoutedEventHandler(VideoPlayerLoaded);
            MediaServer ms = new MediaServer(RenderVideo, Source.ToString(), this);
            ms.Run();
            Ms = null;
            Ms = ms;
            t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 5);
            t.Tick += t_Tick;
        }

        private byte[] RenderVideo(HttpListenerRequest r)
        {
            //get the video bytes from the server etc. and return the same
            return VideoSource;
        }

        /// <summary>
        /// This method runs whent the videoplayer is loaded.
        /// </summary
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlayerLoaded(object sender, RoutedEventArgs e)
        {
            videoPlayer.MediaEnded += delegate(object o, RoutedEventArgs args)
            {
                videoPlayer.Position = new TimeSpan(0, 0, 0, 0);
                videoPlayer.Play();
            };

            _playTimer = new Timer { Interval = 300 };
            _playTimer.Elapsed += delegate(object o, ElapsedEventArgs args)
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => CurrentVideoProgress = videoPlayer.Position.TotalMilliseconds / totalVideoDuration));
                }
                catch (Exception ex)
                { }
            };
            PlayerControls.IsEnabled = false;
            PlayerControls.Opacity = 0;
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The video is ran from the startingpoint. The pausebutton is revealed and the play button hidden.
        /// </summary
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_rewind_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Position = new TimeSpan(0, 0, 0, 0);
            PlayButton.Visibility = Visibility.Collapsed;
            if (btn_play.Visibility != Visibility.Visible)
            {
                btn_play.Visibility = Visibility.Collapsed;
                btn_pause.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// When the pausebutton is clicked it is hidden and the play button is revealed. The Video is paused.
        /// </summary
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            btn_play.Visibility = Visibility.Visible;
            btn_pause.Visibility = Visibility.Collapsed;
            videoPlayer.Pause();
            _playTimer.Stop();
            VideoIsPlaying = false;
        }

        /// <summary>
        /// When the playbutton is clicked it is hidden and the pause button is revealed. The video plays.
        /// </summary
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.Visibility = Visibility.Collapsed;
            btn_play.Visibility = Visibility.Collapsed;
            btn_pause.Visibility = Visibility.Visible;
            videoPlayer.Play();
            _playTimer.Start();
            VideoIsPlaying = true;
        }

        /// <summary>
        /// When the playbutton is clicked it is hidden and the pause button is revealed. The video plays.
        /// </summary
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.Visibility = Visibility.Collapsed;
            btn_play.Visibility = Visibility.Collapsed;
            btn_pause.Visibility = Visibility.Visible;
            videoPlayer.Play();
            _playTimer.Start();
            VideoIsPlaying = true;
            Overlay.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// This method reveals the playercontrols when a touchpoint enters the videoplayer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (PlayerControls.IsEnabled == false)
            {
                PlayerControls.IsEnabled = true;
                animation(1);
                t.Start();
            }
            else
            {
                PlayerControls.IsEnabled = false;
                animation(0);
            }  

        }

        private void t_Tick(object sender, EventArgs e)
        {
            t.Stop();
            PlayerControls.IsEnabled = false;
            animation(0);
        }

        private void animation(int fade)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(fade, TimeSpan.FromSeconds(1));
            PlayerControls.BeginAnimation(SysImages.OpacityProperty, fadeOut);
            
        }

        public void playPauseMethod()
        {
            switch (isPlaying)
            {
                case false:
                    isPlaying = true;
                    PlayButton.Visibility = Visibility.Collapsed;
                    btn_play.Visibility = Visibility.Collapsed;
                    btn_pause.Visibility = Visibility.Visible;
                    videoPlayer.Play();
                    _playTimer.Start();
                    VideoIsPlaying = true;
                    Overlay.Visibility = Visibility.Hidden;
                    break;

                case true:
                    isPlaying = false;
                    btn_play.Visibility = Visibility.Visible;
                    btn_pause.Visibility = Visibility.Collapsed;
                    videoPlayer.Pause();
                    _playTimer.Stop();
                    VideoIsPlaying = false;
                    break;
            }



        }

        private void PlayPauseEvent(object sender, RoutedEventArgs e)
        {
            playPauseMethod();
        }

        #endregion

        /// <summary>
        /// When the videoplayer is loaded it starts at the beginning av stop when finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //PlayButton.Visibility = Visibility.Collapsed;
            ((MediaElement)sender).Play();
            ((MediaElement)sender).Position = new TimeSpan(0, 0, 0, 1);
            ((MediaElement)sender).Pause();
        }

        /// <summary>
        /// This method changes the video when running.
        /// </summary>
        /// <param name="newVideo"></param>
        public void ChangeVideoAtRuntime(byte[] newVideo)
        {
            this.VideoSource = newVideo;
            this.Source = null;
            this.Source = new Uri("http://localhost:8080/" + GlobalUtilities.LocalhostIndex + "/");
            //GlobalUtilities.incrementIndex();
            Ms.Stop();
            MediaServer ms = new MediaServer(RenderVideo, Source.ToString(), this);
            ms.Run();
            Ms = null;
            Ms = ms;
        }

        /// <summary>
        /// This method...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (videoPlayer.NaturalDuration.HasTimeSpan)
                totalVideoDuration = videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }
    }
}
