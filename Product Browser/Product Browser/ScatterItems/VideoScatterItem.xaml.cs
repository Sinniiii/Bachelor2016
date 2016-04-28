using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Core;
using Microsoft.Surface.Presentation.Input;
using DatabaseModel.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for VideoScatterItem.xaml
    /// </summary>
    public partial class VideoScatterItem : ScatterViewItem, INotifyPropertyChanged
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

        DispatcherTimer videoControlTimer;

        #endregion

        #region Properties

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying; }
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
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                _isPlaying = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsPlaying");
            }
        }

        #endregion

        #region EventHandlers

        protected void VideoEndedHandler(object obj, RoutedEventArgs args)
        {
            videoPlayer.Position = new TimeSpan(0, 0, 0);
        }

        protected void VideoControlTick(object obj, EventArgs args)
        {

        }

        #endregion

        public VideoScatterItem(SmartCardDataItem video)
        {
            InitializeComponent();

            this.video = video;

            videoPlayer.LoadedBehavior = MediaState.Manual;
            videoPlayer.UnloadedBehavior = MediaState.Manual;
            videoPlayer.MediaEnded += VideoEndedHandler;
            videoPlayer.Source = video.GetVideo();

            videoControlTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            videoControlTimer.Interval = new TimeSpan(0, 0, 1);
            videoControlTimer.Tick += VideoControlTick;
        }

        private void PauseButtonClicked(object sender, RoutedEventArgs e)
        {
            if (videoPlayer.CanPause)
            {
                IsPaused = true;
                videoPlayer.Pause();
            }
        }

        private void PlayButtonClicked(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
            videoPlayer.Play();
        }
    }
}
