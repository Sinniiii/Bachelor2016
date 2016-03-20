using System;
using System.Collections.Generic;
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
using System.IO;
using ProductBrowser;
using System.Collections.ObjectModel;
using Video;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.ComponentModel;
using System.ServiceProcess;

namespace ProductBrowser
{
    //hello
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : TagVisualization
    {
        ABBDataClassesDataContext dc = new ABBDataClassesDataContext();

        Table<Image> imgs;

        Table<Tag> tags;

        Table<Video> videos;

        Table<Document> documents;

        Table<Offering> offerings;

        private string offeringName;

        private string parent;

        private int offeringID;

        private int parentID;

        private int restartMssqlServiceAttempt;

        private string categoryName;

        private string[] colors = { "0096EA", "5BD8FF", "00ACB6", "39E8DA", "EECE23", "F4EE7F", "FF6C00", "FDAC25" };

        private DocumentLibrary docLib;

        private Grid docGrid;
        private bool VRCard = false;

        //private VideoContainer videoBox;

        private BackgroundWorker selectThread;
        private BackgroundWorker docLibThread;
        private BackgroundWorker videosThread;
        private BackgroundWorker picturesThread;

        /// <summary>This contains an object of the type MainWindow.</summary>
        private MainWindow mainWindow;
        /// <summary>This contains an object of the type PictureContainer.</summary>
        private PictureContainer PictureBox;
        /// <summary>This contains an object of the type VideoContainer.</summary>
        private VideoContainer VideoBox;
        /// <summary>This contains an object of the type VirtualCard.</summary>
        private VirtualCard virtualCard;
        /// <summary>This contains an object of the type Color.</summary>
        private Color tagColour;
        /// <summary>This contains an object of the type Level.</summary>
        private Label lbl_Level;

        /// <summary>Varible that holds an error message .</summary>
        //private string productNotDefined = "Not defined";

        private List<DocumentContainer> openedDocuments;

        public List<ScatterViewItem> listOfSingleImages;

        
        /// <summary>Enum listing possible filetypes.</summary>
        //  private enum FileType { Documents, Pictures, Videos };


        private System.Timers.Timer lostTagTimer;
        /// <summary>Variable of the type boolean that holds the value of if the tag is placed or not.</summary>
        public bool tagIsPlaced { get; set; }

        /// <summary>
        /// Constructor initializes the tag window graphics and funcionality.
        /// </summary>
        public TagWindow()
        {
            InitializeComponent();

            docLib = new DocumentLibrary();
            openedDocuments = new List<DocumentContainer>();
            offeringID = -1;
            restartMssqlServiceAttempt = 0;
            offeringName = "";
            categoryName = "";
            virtualCard = new VirtualCard(this, mainWindow);
            virtualCard.Width = 190;
            virtualCard.Height = 120;
            this.TagGUI.Children.Add(virtualCard);
            this.AllowDrop = false;
            this.tagIsPlaced = true;
            listOfSingleImages = new List<ScatterViewItem>();
        }

        public void setTagDataValues()
        {

            long tagValue = this.VisualizedTag.Value;
            try
            {
                var tagData = tags.
                    Select(data => new { data.TagID, data.OfferingID, data.OfferingName, data.IsEditable, data.Offering.ParentID, data.Offering.Parent, data.Offering.Category }).
                    Where(data => data.TagID == tagValue + 1).First();

                offeringName = tagData.OfferingName;
                offeringID = (int)tagData.OfferingID;
                categoryName = tagData.Category;
                parent = tagData.Parent;
                parentID = (int)tagData.ParentID;
            }
            catch { }
        }

        #region Events

        /// <summary>
        /// This method runs when an item in the list box is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void lbi_Document_Click(object sender, TouchEventArgs e)
        {
            SurfaceListBoxItem sb = sender as SurfaceListBoxItem;
            offeringName = sb.Content.ToString();
            try
            {
                var query = offerings.Select(x => new { x.OfferingID, x.ParentID, x.OfferingName }).Where(x => x.ParentID == offeringID && x.OfferingName.Equals(offeringName)).FirstOrDefault();
                offeringID = query.OfferingID;
            }
            catch { }
            
            RefreshLevelContent();

        }


        /// <summary>
        /// This method returns the colour associated with the product
        /// </summary>
        /// <returns></returns>
        public Color GetTagColour()
        {
            return this.tagColour;
        }

        /// <summary>
        /// This method hides or shows the videoplayer when the button "videos" is clicked on the table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_videos_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (!mainWindow.ScatterWindow.Items.Contains(VideoBox))
            {
                try
                {

                    updateVideos(e);
                    videosThread.RunWorkerCompleted += (senderObjects, args) =>
                    {
                        mainWindow.ScatterWindow.Items.Add(VideoBox);
                    };

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                //This method hides or shows the picturecontainer when the button "pictures" on the table is clicked.
                switch (VideoBox.Visibility)
                {
                    case Visibility.Hidden:
                        VideoBox.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        //VideoBox.PauseVideo();
                        VideoBox.videoControl.videoPlayer.Pause();
                        VideoBox.videoControl._playTimer.Stop();
                        VideoBox.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        private void btn_pictures_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (!mainWindow.ScatterWindow.Items.Contains(PictureBox))
            {
                try
                {

                    updatePictures(e);
                    picturesThread.RunWorkerCompleted += (senderObjects, args) =>
                    {
                        mainWindow.ScatterWindow.Items.Add(PictureBox);

                    };

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {

                //This method hides or shows the picturecontainer when the button "pictures" on the table is clicked.
                switch (PictureBox.Visibility)
                {
                    case Visibility.Hidden:
                        //updatePictures(e);
                        PictureBox.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        PictureBox.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        /// <summary>
        /// This method runs when the lock button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Lock_Click(object sender, RoutedEventArgs e)
        {
            btn_Lock.Visibility = Visibility.Collapsed;
            btn_Open.Visibility = Visibility.Visible;
            mainWindow.LockTagWindow(this);


        }
        /// <summary>
        /// This method runs when the unlock button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            btn_Open.Visibility = Visibility.Collapsed;
            btn_Lock.Visibility = Visibility.Visible;
            /*if (!tagIsPlaced)
            {
                btn_Lock.Visibility = Visibility.Collapsed;
         
            }*/
            mainWindow.UnlockTagWindow(this);

        }


        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var getParent = offerings.Select(x => new { x.Parent, x.ParentID, x.OfferingID }).Where(x => x.OfferingID == offeringID).FirstOrDefault();
                offeringName = getParent.Parent;
                offeringID = (int)getParent.ParentID;

                RefreshLevelContent();

                if (btn_Forward.Visibility == Visibility.Hidden)
                {
                    btn_Forward.Visibility = Visibility.Visible;
                    lbx_Documents.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private void btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            switch (lbx_Documents.Visibility)
            {
                case Visibility.Hidden:
                    lbx_Documents.Visibility = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    lbx_Documents.Visibility = Visibility.Hidden;
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method runs when the product is loaded. It calls private methods to collect information and adds this into the products.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //solacode
        private void TagWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: customize ProductVisualization's UI based on this.VisualizedTag here
            //This method runs when the product is loaded. It calls private methods to collect information
            //and adds this into the products.

            if (this.VisualizedTag.Value != 255)
            {
                mainWindow.LockTagWindow(this);
                try
                {

                    tags = dc.GetTable<Tag>();
                    imgs = dc.GetTable<Image>();
                    offerings = dc.GetTable<Offering>();
                    videos = dc.GetTable<Video>();
                    documents = dc.GetTable<Document>();

                }
                catch (Exception err)
                {
                    MainWindow.warningWindow.Show(err.ToString());
                }

                setTagDataValues();
                SetTagInfo();
                CreateLevelLabel();

                //updateDocLib();
                //docGrid = docLib.DocumentGrid(offeringName, this, mainWindow);
                //AddToListBox();
                RefreshLevelContent();
            }


        }

        /// <summary>
        /// This method adds a label that contains the level name and displays it under the product name.
        /// </summary>
        private void CreateLevelLabel()
        {
            lbl_Level = new Label();
            lbl_Level.Height = 25;
            lbl_Level.Content = offeringName;
            lbl_Level.VerticalAlignment = VerticalAlignment.Top;
            lbl_Level.HorizontalAlignment = HorizontalAlignment.Left;
            lbl_Level.Margin = new Thickness(325, 75, 0, 0);
            //lbl_Level.PreviewTouchDown += lbl_Level_Click;
            this.TagGUI.Children.Add(lbl_Level);
            try
            {
                if (offeringName.Equals(categoryName))
                {
                    lbl_Level.Visibility = Visibility.Hidden;
                    btn_Back.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception e)
            {
                MainWindow.warningWindow.Show(e.ToString());
            }
        }
        /// <summary>
        /// This method removes the video and picture boxs, and updates the document list.
        /// </summary>
        // solacode
        private void RefreshLevelContent()
        {
            lbl_Level.Content = offeringName;

            long tagValue = this.VisualizedTag.Value;
            try
            {
                // var catName = offerings.Select(x => new { x.Category, x.OfferingName, x.OfferingID,x.Parent,x.ParentID }).Where(x => x.OfferingName.Equals(offeringName)).FirstOrDefault();

                var query = offerings.Select(x => new { x.OfferingName, x.OfferingID, x.Category, x.Parent, x.ParentID }).Where(x => x.OfferingID == offeringID).FirstOrDefault();

                var tagData = tags.
                    Select(data => new { data.TagID, data.OfferingID, data.OfferingName, data.IsEditable, data.Offering.ParentID, data.Offering.Parent, data.Offering.Category }).
                    Where(data => data.TagID == tagValue + 1).First();

                if (query != null)
                {
                    offeringName = query.OfferingName;
                    offeringID = (int)query.OfferingID;
                    categoryName = query.Category;
                    parent = query.Parent;
                    parentID = (int)query.ParentID;
                }
                /*
                if (tagData != null)
                {
                    offeringName = tagData.OfferingName;
                    offeringID = (int)tagData.OfferingID;
                    categoryName = tagData.Category;
                    parent = tagData.Parent;
                    parentID = (int)tagData.ParentID;
                }
                    */
                else
                {
                    offeringName = "";
                    offeringID = -1;
                }
            }
            catch { }

            try
            {
                var document = documents.Select(x => new { x.OfferingID }).Where(x => x.OfferingID.Equals(offeringID)).FirstOrDefault();
                var image = imgs.Select(x => new { x.OfferingID }).Where(x => x.OfferingID.Equals(offeringID)).FirstOrDefault();
                var video = videos.Select(x => new { x.OfferingID }).Where(x => x.OfferingID.Equals(offeringID)).FirstOrDefault();

                if (image == null)
                {
                    btn_pictures.IsEnabled = false;
                }
                else
                {
                    btn_pictures.IsEnabled = true;
                }


                if (document == null)
                {
                    btn_documents.IsEnabled = false;
                }
                else
                {
                    btn_documents.IsEnabled = true;
                }


                if (video == null)
                {
                    btn_videos.IsEnabled = false;
                }
                else
                {
                    btn_videos.IsEnabled = true;
                }


                if (lbl_Level.Content.Equals(categoryName))
                {
                    lbl_Level.Visibility = Visibility.Hidden;
                    btn_Back.Visibility = Visibility.Hidden;
                    lbl_ProductName.Content = categoryName;
                }
                else if (offeringID == -1)
                {
                    lbl_Level.Visibility = Visibility.Visible;
                    btn_Forward.Visibility = Visibility.Collapsed;
                    btn_Back.Visibility = Visibility.Collapsed;
                }
                else
                {

                    lbl_Level.Visibility = Visibility.Visible;
                    btn_Back.Visibility = Visibility.Visible;
                }

                if (docGrid != null)
                {
                    //docGrid.Visibility = Visibility.Hidden;
                    mainWindow.ScatterWindow.Items.Remove(docGrid);
                    docGrid = null;
                }
                if (VideoBox != null)
                {
                    //VideoBox.Visibility = Visibility.Hidden;
                    mainWindow.ScatterWindow.Items.Remove(VideoBox);
                    VideoBox = null;
                }

                if (PictureBox != null)
                {
                    //PictureBox.Visibility = Visibility.Hidden;
                    mainWindow.ScatterWindow.Items.Remove(PictureBox);
                    PictureBox = null;
                }
            }
            catch 
            {
                DBHelper.RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                    RefreshLevelContent();
                }
                catch 
                {
                    MainWindow.warningWindow.Show("Failed to refresh content. Please try again. ");
                }
            }
            lbx_Documents.Items.Clear();
            mainWindow.ScatterWindow.Items.Remove(VideoBox);
            AddToListBox();
            //updateVideos(0, false);
        }


        private void AddToListBox()
        {
            offerings = dc.GetTable<Offering>();
            try
            {
                var subCategory = offerings.Select(data => new { data.ParentID, data.OfferingName }).Where(data => data.ParentID == offeringID);
                if (subCategory.FirstOrDefault() != null)
                {
                    foreach (var item in subCategory)
                    {
                        SurfaceListBoxItem lbi_Document = new SurfaceListBoxItem();
                        lbi_Document.Content = item.OfferingName;
                        lbi_Document.FontSize = (int)MainWindow.configurationData.FontSize;
                        lbi_Document.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
                        lbi_Document.Background = new LinearGradientBrush(
                            new GradientStopCollection {
                            new GradientStop(Color.FromArgb(0xFF,0x3C,0x3C,0x3C), 0.0), 
                            new GradientStop(Color.FromArgb(0xFF,0x02,0x02,0x02), 1.0)
                        },
                            new System.Windows.Point(0.5, 0),
                            new System.Windows.Point(0.5, 1));
                        lbi_Document.VerticalContentAlignment = VerticalAlignment.Center;
                        lbi_Document.PreviewTouchUp += lbi_Document_Click;
                        lbx_Documents.Items.Add(lbi_Document);
                    }

                }
                else
                {
                    lbx_Documents.Visibility = Visibility.Hidden;
                    btn_Forward.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception e)
            {
                DBHelper.RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                    AddToListBox();
                }
                catch (Exception ex)
                {

                    MainWindow.warningWindow.Show("Failed to add sublevels to listbox. Please try again. ");
                }
            }
        }

        /// <summary>
        /// This method extracts the product from the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ExtractNameFromPath(string path)
        {
            string[] substrings = path.Split('\\');
            return substrings[substrings.Length - 1];
        }
        /// <summary>
        /// This method reads the languages and sets the labels in the designer.
        /// </summary>
        /// <param name="list"></param>
        private void ReadLanguage(List<StringData> list)
        {
            foreach (StringData sd in list)
            {
                if (sd.stringTag == "btn_document")
                    btn_documents.Content = sd.stringValue;
                else if (sd.stringTag == "btn_video")
                    btn_videos.Content = sd.stringValue;
                else if (sd.stringTag == "btn_picture")
                    btn_pictures.Content = sd.stringValue;
            }
        }

        /// <summary>
        /// This method set the colour used for this tag's frames.
        /// </summary>
        private void SetTagInfo()
        {

            try
            {
                int randNumber = random();
                string colorString = colors[randNumber];
                tagColour = (Color)ColorConverter.ConvertFromString("#" + colorString);

                lbl_ProductName.Content = categoryName;
            }
            catch (Exception ex)
            {

                // lbl_ProductName.Content = productNotDefined;
            }


        }

        /// <summary>
        /// This method sets the buttons to enabled/disabled.
        /// </summary>
        /// <param name="foundDocs"></param>
        /// <param name="foundVideos"></param>
        /// <param name="foundPics"></param>
        private void EnableDisableButtons(string[] foundDocs, string[] foundVideos, string[] foundPics)
        {
            if (foundDocs == null)
                btn_documents.IsEnabled = false;
            else
                btn_documents.IsEnabled = true;

            if (foundVideos == null)
                btn_videos.IsEnabled = false;
            else
                btn_videos.IsEnabled = true;

            if (foundPics == null)
                btn_pictures.IsEnabled = false;
            else
                btn_pictures.IsEnabled = true;
        }


        /// <summary>
        /// This method returns a string that contains the level to search at.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string GetLevelLine(string s)
        {
            char separator = ';';
            string[] parts = s.Split(separator);
            return parts[0];
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method run when a tag is removed from the table.
        /// </summary>
        public void TagIsLost()
        {
            if (this.TagRemovedBehavior.Equals(TagRemovedBehavior.Fade))
            {
                lostTagTimer = new System.Timers.Timer();
                lostTagTimer.Interval = (double)MainWindow.configurationData.LostTagTimeout + 0.1;
                lostTagTimer.Enabled = true;
                if (VRCard)
                {
                    this.tagIsPlaced = false;
                }
                lostTagTimer.Start();
                lostTagTimer.Elapsed += (sender, args) =>
                {
                    if (!this.tagIsPlaced)
                    {
                        Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                           if (mainWindow.ScatterWindow.Items.Contains(VideoBox))
                            {

                                mainWindow.ScatterWindow.Items.Remove(VideoBox);
                                VideoBox = null;
                            }
                            if (mainWindow.ScatterWindow.Items.Contains(PictureBox))
                            {

                                mainWindow.ScatterWindow.Items.Remove(PictureBox);
                                //PictureBox.ClearSingleImages();
                                PictureBox = null;
                                
                            }
                            if (mainWindow.ScatterWindow.Items.Contains(docGrid))
                            {
                                mainWindow.ScatterWindow.Items.Remove(docGrid);
                                docGrid = null;
                            }
                            
                            

                           foreach (DocumentContainer documentBox in openedDocuments)
                            {
                                //documentBox.ClearSinglePages();
                                mainWindow.ScatterWindow.Items.Remove(documentBox);


                            }

                           foreach (var item in listOfSingleImages)
                           {

                               mainWindow.ScatterWindow.Items.Remove(item);
                           }
                            
                            mainWindow.ScatterWindow.Items.Remove(this);
                            if (mainWindow.ScatterWindow.Items.Count <= 0)
                            {
                                Dispatcher.BeginInvoke(new ThreadStart(() => mainWindow.BlurEffect.Radius = 0));
                                Dispatcher.BeginInvoke(new ThreadStart(() => mainWindow.BlurEffectBack.Radius = 0));
                            }




                        }));

                        lostTagTimer.Stop();
                    }
                };
            }
        }

        /// <summary>
        /// This method sets the tag is placed property.
        /// </summary>
        public void TagIsPlaced()
        {
            tagIsPlaced = true;
            if (lostTagTimer != null)
                lostTagTimer.Stop();
        }

        /// <summary>
        /// This method sets a reference to the main window.
        /// </summary>
        /// <param name="mainWindow"></param>
        public void SetMainWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        /// <summary>
        /// This method returns the name of the product
        /// </summary>
        /// <returns></returns>
        public string GetProductName()
        {
            return this.offeringName;
        }


        public void addDoc(int documentId, string title, TagWindow tagWindow, MainWindow mainWindow, SurfaceDragCursor droppingCursor, Point position)
        {
            DocumentContainer documentBox = new DocumentContainer(documentId, title, tagWindow, mainWindow);

            docGrid = docLib.DocumentGrid(offeringName, this, mainWindow);
            documentBox.Center = position;
            documentBox.Orientation = droppingCursor.GetOrientation(this);

            documentBox.SetRelativeZIndex(RelativeScatterViewZIndex.Topmost);

            openedDocuments.Add(documentBox);

        }

        #endregion

        private void btn_documents_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (!mainWindow.ScatterWindow.Items.Contains(docGrid))
            {
                try
                {
                    //AddToListBox();

                    //Point point = e.TouchDevice.GetPosition(this);
                    docGrid = new Grid();
                    updateDocLib();
                    docLibThread.RunWorkerCompleted += (senderObjects, args) =>
                    {
                        try
                        {
                            ((Window)docGrid.Parent).Content = null;
                        }
                        catch { }
                        mainWindow.ScatterWindow.Items.Add(docGrid);

                    };

                }
                catch (Exception ex)
                {
                    //The documents could not be added to the list box.
                    MessageBox.Show(ex.Message);
                }
            }
            //This method hides or shows the picturecontainer when the button "pictures" on the table is clicked.
            else
            {
                switch (docGrid.Visibility)
                {
                    case Visibility.Hidden:
                        //updateDocLib();
                        docGrid.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        docGrid.Visibility = Visibility.Hidden;

                        break;
                }
            }
        }


        public int random()
        {

            Random rand = new Random();
            int randomNumber = rand.Next(0, 7);

            return randomNumber;
        }

        private void updateDocLib()
        {
            docLibThread = new BackgroundWorker();
            docLibThread.RunWorkerAsync();
            docLibThread.DoWork += (senderObjects, args) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    docGrid = docLib.DocumentGrid(offeringName, this, mainWindow);
                }));
            };


        }


        private void updateVideos(TouchEventArgs e)
        {
            int numberOfVideos = DBHelper.numberOfVideos(this.offeringName);
            videosThread = new BackgroundWorker();
            videosThread.RunWorkerAsync();
            videosThread.DoWork += (senderObjects, args) =>
            {
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {

                        VideoBox = new VideoContainer(offeringID, numberOfVideos, this, mainWindow);
                        //VideoBox.Visibility = System.Windows.Visibility.Visible;
                        double fingerOrientation = e.TouchDevice.GetOrientation(null) + 90;
                        VideoBox.Orientation = fingerOrientation;
                    }));
                };
            };


        }


        private void updatePictures(TouchEventArgs e)
        {
            picturesThread = new BackgroundWorker();
            picturesThread.RunWorkerAsync();
            picturesThread.DoWork += (senderObjects, args) =>
            {
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        var query =
                         from i in dc.Images
                         where i.OfferingName == this.offeringName
                         select i;

                        int numberOfImages = query.Count();
                        PictureBox = new PictureContainer(offeringID, numberOfImages, this, mainWindow);
                        double fingerOrientation = e.TouchDevice.GetOrientation(null) + 90;
                        PictureBox.Orientation = fingerOrientation;
                    }));
                };
            };

        }

        public bool isVirtualCard
        {

            get
            {
                return VRCard;
            }
            set
            {

                VRCard = value;
            }


        }
    }
}
