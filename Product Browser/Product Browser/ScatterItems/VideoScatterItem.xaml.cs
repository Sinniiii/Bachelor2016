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

namespace Product_Browser.ScatterItems
{
    /// <summary>
    /// Interaction logic for VideoScatterItem.xaml
    /// </summary>
    public partial class VideoScatterItem : ScatterViewItem
    {
        SmartCardDataItem video = null;

        public VideoScatterItem(SmartCardDataItem video)
        {
            InitializeComponent();

            this.video = video;
            //Loaded += VideoScatterItemLoaded;

            videoPlayer.LoadedBehavior = MediaState.Manual;
            videoPlayer.UnloadedBehavior = MediaState.Manual;
            

            videoPlayer.Loaded += VideoScatterItemLoaded;
        }

        protected void VideoScatterItemLoaded(object sender, EventArgs e)
        {
            videoPlayer.Source = video.GetVideo();
            videoPlayer.Play();
        }
    }
}
