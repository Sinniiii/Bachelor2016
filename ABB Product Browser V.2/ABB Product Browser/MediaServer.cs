using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Windows.Data;
using Video;

namespace ProductBrowser
{
    public class MediaServer : INotifyPropertyChanged
    {
        private HttpListener listener;

        private Func<HttpListenerRequest, byte[]> _responderMethod;

        public event PropertyChangedEventHandler PropertyChanged;

        private VideoPlayer videoPlayer;

        public HttpListener Listener
        {
            get
            {
                return listener;
            }
            set
            {
                listener = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Listener"));
            }
        }

        public VideoPlayer VideoPlayer
        {
            get
            {
                return videoPlayer;
            }
            set
            {
                videoPlayer = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("VideoPlayer"));
            }
        }

        public MediaServer(Func<HttpListenerRequest, byte[]> method, string prefix, VideoPlayer videoPlayer)
        {
            VideoPlayer = videoPlayer;
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            if (prefix == null)
                throw new ArgumentException("prefix");


            if (method == null)
                throw new ArgumentException("method");

            this._responderMethod = method;

            //if (!Listener.IsListening

            Listener = new HttpListener();
            Listener.Prefixes.Add(prefix);
            Listener.Start();
        }


        public void Run()
        {

            VideoPlayer.loadingAnimation.TextContent.Text = "Buffering";
            VideoPlayer.loadingAnimation.Visibility = Visibility.Visible;
            //VideoPlayer.PlayButton.Visibility = Visibility.Hidden;
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (Listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                byte[] buf = _responderMethod(ctx.Request);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.ContentType = "application/octet-stream";
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                ctx.Response.OutputStream.Close();
                                //VideoPlayer.PlayButton.Visibility = Visibility.Visible;
                                VideoPlayer.loadingAnimation.Visibility = Visibility.Collapsed;
                            }
                            catch { }
                            finally
                            {
                                
                                //ctx.Response.OutputStream.Close();
                            }
                        }, Listener.GetContext());
                        
                    }
                }
                catch { }
            });
        }

        public void Stop()
        {
            Listener.Stop();
            Listener.Close();
        }
    }
}
