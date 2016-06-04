using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using DatabaseModel.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for VideoScatterItem.xaml
    /// </summary>
    public partial class VideoScatterItem : ABBScatterItem, INotifyPropertyChanged
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

        SmartCardDataItem video = null;

        DispatcherTimer videoControlTimer, controlsVisibleTimer;



        #endregion

        #region Properties

        private bool hasBeenManipulated = false;
        public bool HasBeenManipulated
        {
            get { return hasBeenManipulated; }
            set { hasBeenManipulated = value; NotifyPropertyChanged(); }
        }

        private bool _controlsVisible = false;
        public bool ControlsVisible
        {
            get { return _controlsVisible; }
            set
            {
                _controlsVisible = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsPlaying");
                NotifyPropertyChanged("IsPaused");
            }
        }

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying && ControlsVisible; }
            set {
                _isPlaying = value;
                _isPaused = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsPaused");
            }
        }

        private bool _isPaused = true;
        public bool IsPaused
        {
            get { return _isPaused && ControlsVisible; }
            set
            {
                _isPaused = value;
                _isPlaying = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsPlaying");
            }
        }

        #endregion

        #region ButtonClickHandlers

        private void PauseButtonClicked(object sender, RoutedEventArgs e)
        {
            PauseVideo();

            e.Handled = true;
        }

        private void PlayButtonClicked(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
            videoPlayer.Play();

            e.Handled = true;
        }

        #endregion

        #region EventHandlers

        protected void OnOverlayManipulated(object sender, RoutedEventArgs e)
        {
            HasBeenManipulated = true;
            overlay.Visibility = Visibility.Hidden;
            IsPlaying = true;
            videoPlayer.Play();

            e.Handled = true;
        }

        protected void VideoPlayerLoadedHandler(object obj, EventArgs args)
        {
            videoPlayer.Play(); // Avoid black screen by starting video and triggering media load
            videoPlayer.Stop();

            // Remove shadow of this smartcard, or we get an ugly effect
            var ssc = this.GetTemplateChild("shadow") as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        protected void VideoPlayerMediaLoadedHandler(object obj, EventArgs args)
        {            
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                progressBar.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                progressBar.PreviewMouseDown += ProgressBarMouseDownHandler;
                progressBar.PreviewTouchDown += ProgressBarTouchDownHandler;
            }            
        }

        private void ProgressBarTouchDownHandler(object sender, System.Windows.Input.TouchEventArgs e)
        {
            var touchPoint = e.GetTouchPoint(progressBar).Position;

            GoToVideoLocation(touchPoint.X);
        }

        protected void ProgressBarMouseDownHandler(object obj, MouseButtonEventArgs args)
        {
            var clickPoint = args.GetPosition(progressBar);

            GoToVideoLocation(clickPoint.X);
        }

        protected void VideoEndedHandler(object obj, RoutedEventArgs args)
        {
            videoPlayer.Position = new TimeSpan(0, 0, 0, 0, 100);
        }

        protected void VideoControlTick(object obj, EventArgs args)
        {
            // Handle update of progress bar etc!
            if(videoPlayer.NaturalDuration.HasTimeSpan)
                progressBar.Value = videoPlayer.Position.TotalSeconds; 
        }

        protected void ControlsVisibleTick(object obj, EventArgs args)
        {
            if (IsPaused)
                return;

            ControlsVisible = false;

            videoControlTimer.Stop();
            controlsVisibleTimer.Stop();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if(HasBeenManipulated)
                ShowControls();
        }

        protected override void OnPreviewTouchMove(System.Windows.Input.TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);

            if(HasBeenManipulated)
                ShowControls();
        }

        #endregion

        #region Methods

        private void GoToVideoLocation(double xInput)
        {
            double newPosition = xInput / progressBar.ActualWidth;

            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                int current = (int)(newPosition * progressBar.Maximum);

                videoPlayer.Position = new TimeSpan(0, 0, current);
            }
        }

        private void ShowControls()
        {
            ControlsVisible = true;

            videoControlTimer.Start();
            controlsVisibleTimer.Stop();
            controlsVisibleTimer.Start();
        }

        public void PauseVideo()
        {
            if (videoPlayer.CanPause)
            {
                IsPaused = true;
                videoPlayer.Pause();
            }
        }

        public void KillVideo()
        {
            PauseVideo();

            videoPlayer.Close();
            videoPlayer.Source = null;
        }

        #endregion

        public VideoScatterItem(SmartCardDataItem video)
        {
            InitializeComponent();

            this.video = video;

            videoName.Content = video.Name;

            ((Storyboard)TryFindResource("sb1")).Begin();

            videoPlayer.LoadedBehavior = MediaState.Manual;
            videoPlayer.UnloadedBehavior = MediaState.Manual;
            videoPlayer.Source = video.GetVideo();

            videoPlayer.MediaEnded += VideoEndedHandler;
            videoPlayer.Loaded += VideoPlayerLoadedHandler;
            videoPlayer.MediaOpened += VideoPlayerMediaLoadedHandler;

            videoControlTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            videoControlTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            videoControlTimer.Tick += VideoControlTick;

            controlsVisibleTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            controlsVisibleTimer.Interval = new TimeSpan(0, 0, 0, 0, 1500);
            controlsVisibleTimer.Tick += ControlsVisibleTick;

            overlay.MouseDown += OnOverlayManipulated;
            overlay.TouchDown += OnOverlayManipulated;
        }
        
    }
}
