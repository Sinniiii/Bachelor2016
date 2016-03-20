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
using System.Windows.Media.Animation;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using SysImages = System.Windows.Controls.Image;
using System.Data.Linq;

namespace ProductBrowser
{
    /** Knut is very cool
     * Variable for keeping track of free URIs on the localhost
     */
    public class GlobalUtilities
    {
        //public static HttpListener globalHttpListener = new HttpListener();
        private static int localhostIndex = 0;

        public static int LocalhostIndex
        {
            get
            {
                return localhostIndex;
            }
            set
            {
                localhostIndex = value;
            }
        }

        public static void incrementIndex()
        {
            LocalhostIndex++;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        /// <summary>Variable that the path for the root folder.</summary>
        public static string rootFolder;
        /// <summary>Variable that holds the application language.</summary>
        public static string applicationLanguage;
        /// <summary>Variable that the path for the root folder.</summary>
        //public static string tagFilesDirectory;
        /// <summary>List with strings that holds the tags that are put on the table.</summary>
        private List<string> tagValuesDefined = new List<string>();
        /// <summary>List with languae elements.</summary>
        public static List<StringData> languange = new List<StringData>();
        /// <summary>Object of the type Stopwatch.</summary>
        private readonly Stopwatch lostTagStopwatch = new Stopwatch();
        /// <summary>List with configuration data.</summary>
        public static List<StringData> configData = new List<StringData>();
        /// <summary>Object of the type WarningWindow.</summary>
        public static WarningWindow warningWindow;

        public QuestionWindow questionWindow;

        public ReprogramTags reprogramTags;

        public WatchedLibrary watchLibrary;

        public USBWindow usbWindow;

        public Grid usbGrid = new Grid();

        public Grid grid;

        public Grid watchContainer = new Grid();

        public Grid configGrid;

        private List<byte[]> backgroundList = new List<byte[]>();


        private int pos = 270;
        bool toolbarShow = false;



        public HttpListener GlobalHttpListener { get; set; }

        private System.Windows.Threading.DispatcherTimer t;
        private System.Windows.Threading.DispatcherTimer t2;
        private System.Windows.Threading.DispatcherTimer animationTimer;

        ABBDataClassesDataContext dc = new ABBDataClassesDataContext();
        Table<Setting> settings;
        Table<Background> backgrounds;
        public static Setting configurationData;

        //private System.Windows.Threading.DispatcherTimer t3;


        int counter = 0;

        string[] mainBG = new string[] { "Resources/Backgrounds/BG1.jpg", "Resources/Backgrounds/BG7.jpg", "Resources/Backgrounds/BG3.jpg", "Resources/Backgrounds/BG6.jpg", "Resources/Backgrounds/BG2.jpg" };


        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            GlobalHttpListener = new HttpListener();
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            warningWindow = new WarningWindow(this);
            questionWindow = new QuestionWindow(this);
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            settings = dc.GetTable<Setting>();
            backgrounds = dc.GetTable<Background>();
            configurationData = settings.Where(x => x.SettingName.Equals("Current")).FirstOrDefault();
            t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0, (int)configurationData.BgTimer, 0);
            t.Tick += t_Tick;
            t.Start();

            t2 = new System.Windows.Threading.DispatcherTimer();
            t2.Interval = new TimeSpan(0, 0, 3);
            t2.Tick += t_Tick2;

            animationTimer = new System.Windows.Threading.DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            animationTimer.Tick += t_TickAnimation;

            
            convertByteArray(backgrounds.Select(x => x.BackgroundBinary));
            backgrounds.Select(x => x.BackgroundBinary);
            BitmapSource img = BlobConverter.ConvertImgBlobtoImage(backgroundList.First());
            backgroundList.ElementAt(2);
            BGImage.Source = img;
            //ReadConfigFile();
            configData = DBHelper.ReadConfigSettings();
            SetConfigData();
            ReadLanguageFile(applicationLanguage + ".txt");

            

            mainGrid.AllowDrop = true;

            watchLibrary = new WatchedLibrary();
            watchContainer = watchLibrary.WatchedGrid();
            ((Window)watchContainer.Parent).Content = null;

            usbWindow = new USBWindow();
            usbGrid = usbWindow.USBGrid();
            ((Window)usbGrid.Parent).Content = null;
  
        }





        private void convertByteArray(IQueryable<byte[]> input)
        {
            foreach (var item in input)
            {
                backgroundList.Add(item);
            }
            
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // Use this since we are a WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Use this since we are a console app
                System.Environment.Exit(1);
            }
            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled
            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        /// <summary>
        /// Occurs when the table is touched.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            bool isFinger = e.TouchDevice.GetIsFingerRecognized();
            bool isTag = e.TouchDevice.GetIsTagRecognized();

            if (isFinger == false && isTag == false)
            {
                e.Handled = true;
                return;
            }
            long sdf = e.TouchDevice.GetTagData().Value;
            // && e.TouchDevice.GetTagData().Value != 254
            if (isTag && mainGrid.AllowDrop)
            {
                TagData data = e.TouchDevice.GetTagData();
                long tagValue = data.Value;
                if (tagValue < 254)
                {
                    BlurEffect.Radius = 10;
                    BlurEffectBack.Radius = 10;
                }
                
                InitializeDefinition(tagValue);
                tagValuesDefined.Add(data.Value.ToString("X"));
            }

            base.OnPreviewTouchDown(e);

        }

        /// <summary>
        /// This method is called when a tag is detected on the table.
        /// </summary>
        /// <param name="e"></param>
        private void InitializeDefinition(long e)
        {
            if (tagValuesDefined.Contains(e.ToString("255")))
            {
                tagValuesDefined.Remove(e.ToString("255"));
                foreach (TagVisualizationDefinition td in tagVisualizer.Definitions)
                {
                    if (td.Value == e)
                    {
                        tagVisualizer.Definitions.Remove(td);
                        break;
                    }
                }
            }

            TagVisualizationDefinition tagDef = new TagVisualizationDefinition();
            tagDef.Value = e;
            // The .xaml file for the UI
            if (e.ToString("X") == "FF")
            {
                ScatterWindow.Visibility = System.Windows.Visibility.Collapsed;
                configGrid = new Grid();
                Configurations config = new Configurations(this);
                configGrid = config.Grid;
                ((Window)configGrid.Parent).Content = null;
                mainGrid.Children.Add(configGrid);
                mainGrid.AllowDrop = false;
            }
            else if (e.ToString("X") == "FE")
            {
                ScatterWindow.Visibility = System.Windows.Visibility.Collapsed;
                //ToolBarGrid.Visibility = Visibility.Collapsed;
                grid = new Grid();
                reprogramTags = new ReprogramTags(this);
                grid = reprogramTags.Grid;
                ((Window)grid.Parent).Content = null;
                mainGrid.Children.Add(grid);
                //this.ScatterWindow.IsEnabled = false;
                mainGrid.AllowDrop = false;
                
            }
            else
            {
                tagDef.Source = new Uri("TagWindow.xaml", UriKind.Relative);
                // Physical offset (horizontal inches, vertical inches).
                tagDef.PhysicalCenterOffsetFromTag = new Vector(0, 0);
                // Orientation offset (default).
                tagDef.OrientationOffsetFromTag = (int)configurationData.OrientationOffsetFromTag;
            }
            // The maximum number for this tag value.
            tagDef.MaxCount = 1;
            // The visualization stays for as long as the config file says.
            tagDef.LostTagTimeout = (int)configurationData.LostTagTimeout;
            // Tag removal behavior (default).
            tagDef.TagRemovedBehavior = TagRemovedBehavior.Fade;
            // Orient UI to tag? (default).
            tagDef.UsesTagOrientation = true;
            // Add the definition to the collection.
            tagVisualizer.Definitions.Add(tagDef);
        }

        /// <summary>
        /// This method is called when a tagvisualizer is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagVisualization_Added(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization newTagVisualization = e.TagVisualization as TagVisualization;
            if ((newTagVisualization.VisualizedTag.Value != 254) && (newTagVisualization.VisualizedTag.Value != 255))
            {
                TagWindow newTagWindow = newTagVisualization as TagWindow;
                newTagWindow.SetMainWindow(this);
                newTagWindow.LostTag += new RoutedEventHandler(OnLostTag);
                newTagWindow.GotTag += new RoutedEventHandler(newTagWindow_GotTag);
                newTagWindow.tagIsPlaced = true;
               
            }
            /*if (newTagVisualization.VisualizedTag.Value == 255)
            {
                ConfigWindow newConfigWindow = newTagVisualization as ConfigWindow;
                newConfigWindow.SetMainWindow(this);
                newConfigWindow.LostTag += new RoutedEventHandler(newConfigWindow.TagVisualization_LostTag);
            }*/

             
        }

        /// <summary>
        /// This method is called when a tag is removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLostTag(object sender, RoutedEventArgs e)
        {

            TagWindow TagWindowOfLostTag = e.Source as TagWindow;
            TagWindowOfLostTag.tagIsPlaced = false;
            TagWindowOfLostTag.TagIsLost();
            lostTagStopwatch.Restart();
          
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void newTagWindow_GotTag(object sender, RoutedEventArgs e)
        {
            TagWindow TagWindowOfLostTag = e.Source as TagWindow;
            TagWindowOfLostTag.TagIsPlaced();
        }

        /// <summary>
        /// This method reads the file that sets the language in the program.
        /// </summary>
        /// <param name="filename"></param>
        public static void ReadLanguageFile(string filename)
        {
            try
            {
                languange.Clear();
                StreamReader filereader = File.OpenText(filename);
                string line;
                while ((line = filereader.ReadLine()) != null)
                {
                    StringData sd = new StringData();
                    char separator = ';';
                    string[] parts = line.Split(separator);
                    sd.stringTag = parts[0];
                    sd.stringValue = parts[1];
                    languange.Add(sd);
                }
                filereader.Close();
            }
            catch (Exception ex)
            {
                //MainWindow.warningWindow.Show(ex.Message);
            }
        }

        /// <summary>
        /// This method reads the file that sets the language in the program.
        /// </summary>
        /// <param name="filename"></param>
        /*public static void ReadConfigFile()
        {
            try
            {
                configData.Clear();
                StreamReader filereader = File.OpenText("Config.txt");
                filereader.BaseStream.Position = 0;
                string line;
                while ((line = filereader.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        StringData sd = new StringData();
                        char separator = ';';
                        string[] parts = line.Split(separator);
                        sd.stringTag = parts[0];
                        sd.stringValue = parts[1];
                        configData.Add(sd);
                    }
                }
                filereader.Close();
                SetConfigData();
            }
            catch (Exception ex)
            {
                MainWindow.warningWindow.Show(ex.Message);
            }
        }*/

        /// <summary>
        /// This method is called by the constructor to set data found in the config file.
        /// </summary>
        /// <param name="filename"></param>
        private static void SetConfigData()
        {
            rootFolder = configurationData.RootFolder;
            //tagFilesDirectory = configData.Find(x => x.stringTag.Contains("TagFilesPath")).stringValue;
            applicationLanguage = configurationData.Language;
        }


        /// <summary>
        /// This method locks the tag window on the table.
        /// </summary>
        /// <param name="tagWindow"></param>
        public void LockTagWindow(TagWindow tagWindow)
        {
            tagWindow.TagRemovedBehavior = TagRemovedBehavior.Wait;
            tagWindow.LostTagTimeout = double.PositiveInfinity;
            ScatterWindow.Items.Refresh();
        }
        /// <summary>
        /// This method unlocks the tag window on the table.
        /// </summary>
        /// <param name="tagWindow"></param>
        public void UnlockTagWindow(TagWindow tagWindow)
        {
            tagWindow.TagRemovedBehavior = TagRemovedBehavior.Fade;
            TimeSpan timeSpan = lostTagStopwatch.Elapsed;
            tagWindow.LostTagTimeout = timeSpan.Milliseconds + 2000;

            tagWindow.TagIsLost();
            tagWindow.UpdateLayout();
        }
        /// <summary>
        /// This method refreshes the content.
        /// </summary>
        public void RefreshContent()
        {
            try
            {
                this.mainGrid.Children.Remove(MainWindow.warningWindow);
                //this.mainGrid.Children.Remove(this.grid);////////////
                //this.ScatterWindow.IsEnabled = true;
                //mainGrid.AllowDrop = true;
                this.UpdateLayout();
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message);
            }
        }
        #region Toolbar
        
        private void btn_NewSmartCard_Click(object sender, RoutedEventArgs e)
        {
            //long cardId = 4;
            btn_Show_Click(sender, e);
            //InitializeDefinition(0);
            //tagValuesDefined.Add(cardId.ToString("X"));
            
            TagWindow newTagWindow = new TagWindow();
            
            newTagWindow.SetMainWindow(this);
            newTagWindow.LostTag += new RoutedEventHandler(OnLostTag);
            newTagWindow.GotTag += new RoutedEventHandler(newTagWindow_GotTag);
            newTagWindow.TagRemovedBehavior = TagRemovedBehavior.Fade;
            newTagWindow.tagIsPlaced = false;
            newTagWindow.isVirtualCard = true;
            ScatterWindow.Items.Add(newTagWindow);
            BlurEffect.Radius = 10;
            BlurEffectBack.Radius = 10;

        }
        private void btn_LogOut_Click(object sender, RoutedEventArgs e)
        {

           
            btn_Show_Click(sender, e);

        }
        private void btn_NFC_Container_Click(object sender, RoutedEventArgs e)
        {
            if (!ScatterWindow.Items.Contains(watchContainer))
            {
                ScatterWindow.Items.Add(watchContainer);
            }
            else
            {
                switch (watchContainer.Visibility)
                {
                    case Visibility.Collapsed:
                        watchContainer.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        watchContainer.Visibility = Visibility.Collapsed;
                        break;
                }
            }


            
            btn_Show_Click(sender, e);

        }
        private void btn_Clean_Click(object sender, RoutedEventArgs e)
        {

            ScatterWindow.Items.Clear();
            btn_Show_Click(sender, e);

        }

        private void btn_EventLog_Click(object sender, RoutedEventArgs e)
        {

            if (!ScatterWindow.Items.Contains(usbGrid))
            {
                ScatterWindow.Items.Add(usbGrid);
            }
            else
            {
                switch (usbGrid.Visibility)
                {
                    case Visibility.Collapsed:
                        usbGrid.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        usbGrid.Visibility = Visibility.Collapsed;
                        break;
                }
            }

            btn_Show_Click(sender, e);

        }
  

        private void btn_Show_Click(object sender, RoutedEventArgs e)
        {
            switch (toolbarShow)
            {
                case true:
                    btn_show.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Resources/btnNavForward.png")));
                    toolbarShow = false;
                    break;

                case false:
                    btn_show.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Resources/btnNavBack.png")));
                    toolbarShow = true;
                    break;


            }

            animationTimer.Start();
        }

        private void t_TickAnimation(object sender, EventArgs e)
        {
            double angle = (Math.PI * pos) / 180.0;
            double posX = ((Math.Sin(angle) * 75) - 75);
            switch (toolbarShow)
            {
                case false:
                    toolBar.Margin = new Thickness(posX, 0, 0, 0);
                    btn_show.Margin = new Thickness(posX + 147, -2, 0, 0);
                    if (pos >= 270)
                        animationTimer.Stop();
                    else
                        pos = pos + 3;
                    break;

                case true:

                    toolBar.Margin = new Thickness(posX, 0, 0, 0);
                    btn_show.Margin = new Thickness(posX + 147, -2, 0, 0);
                    if (pos <= 90)
                        animationTimer.Stop();
                    else
                        pos = pos - 3;
                    break;

            }


        }

        #endregion

        #region SlideShow
        private void t_Tick(object sender, EventArgs e)
        {

            BitmapSource img;
            t2.Start();

            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(3));
            SysImages bg = BGImage2;

            if (counter == backgroundList.Count - 1)
            {
                img = BlobConverter.ConvertImgBlobtoImage(backgroundList.First());
                BGImage.Source = img;

            }
            else
            {
                img = BlobConverter.ConvertImgBlobtoImage(backgroundList.ElementAt(counter + 1));
                BGImage.Source = img;
            }

            //BGImage2.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), mainBG[counter]));
            img = BlobConverter.ConvertImgBlobtoImage(backgroundList.ElementAt(counter));
            BGImage2.Source = img;

            bg.BeginAnimation(SysImages.OpacityProperty, fadeOut);

            counter++;


            if (counter == mainBG.Length)
            {

                counter = 0;
            }
  

        }

        private void t_Tick2(object sender, EventArgs e)
        {
            t2.Stop();
            //t2.Tick -= t_Tick2;
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(1));

            System.Windows.Controls.Image bg = BGImage2;

            //BGImage2.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), mainBG[counter]));
            BitmapSource img = BlobConverter.ConvertImgBlobtoImage(backgroundList.ElementAt(counter));
            BGImage2.Source = img;

            bg.BeginAnimation(SysImages.OpacityProperty, fadeIn);



        }
     
        #endregion

    }
}
