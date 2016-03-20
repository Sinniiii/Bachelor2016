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
using System.Data.Linq;
using System.Data;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.ServiceProcess;
using Dolinay;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for ReprogramTags.xaml
    /// </summary>
    public partial class ReprogramTags : SurfaceWindow, INotifyPropertyChanged
    {
        /// <summary>Database connection </summary>
        ABBDataClassesDataContext dc = new ABBDataClassesDataContext();

        /// <summary>Table offerings from databse table</summary>
        Table<Offering> offerings;

        /// <summary>Table tag from databse table</summary>
        Table<Tag> tags;

        /// <summary>Table document from databse table</summary>
        Table<Document> documents;

        /// <summary>Table image from databse table</summary>
        Table<Image> images;

        /// <summary>Table video from databse table</summary>
        Table<Video> videos;

        Table<Category> category;

        DriveDetector driveDetector; 

        /// <summary>Default offering name at startup</summary>
        private string offeringName = "Product Guide";

        private string parent;

        private int offeringID;

        private int parentID;

        private string[] videoFormats = {".avi", ".wmv", ".mov", ".mp4", ".h.264", ".avchd"};
        private string[] imageFormats = { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };

        /// <summary>Solid brush color</summary>
        SolidColorBrush brush = new SolidColorBrush(Colors.LightGray);

        /// <summary>Color red/blue if editing is allowed</summary>
        Color canEditColor;

        SolidColorBrush DarkBlue = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x70, 0xEA));
        SolidColorBrush Azure = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF));
        SolidColorBrush White = new SolidColorBrush(Colors.White);

        private string aCategory;

        /// <summary>int that keep track of how all tagcards on canvas square</summary>
        private int enterCount;

        /// <summary>int value of first tag dropped on square canvas</summary>
        private int tagValue;

        /// <summary>FolderBrowserDialog window variable for folderbrowsing</summary>
        private System.Windows.Forms.FolderBrowserDialog FolderDialog;

        /// <summary>boolean variable determing if folderDialg is showing</summary>
        private bool folderDialogIsShown;

        /// <summary>int to indicate if doubletap occured, is regulated by a timerevent</summary>
        private int doubletap = 0;

        /// <summary>int of current category in categories list</summary>
        private int categoryIndex;

        /// <summary>Numbers of categories</summary>
        private int numberOfCategories;

        /// <summary>list of categories at current level</summary>
        private List<string> categories;

        /// <summary>Object to determin if double click occured on same object</summary>
        private object tappedObject;

        /// <summary>Timer thread for timer event</summary>
        private System.Windows.Threading.DispatcherTimer t;

        /// <summary>Thread used when searching filedirectory for files</summary>
        private BackgroundWorker searchThread;

        /// <summary>The filetype that is currently used</summary>
        private FileType currentFileType;

        // private SurfaceListBoxItem selectedItem;

        private string currentFileTypeString;

        /// <summary>The Different filetypes used in currentFileType</summary>
        private enum FileType { Pictures, Videos, Documents };

        /// <summary>Thread used when saving files to database</summary>
        private BackgroundWorker createThread;

        private BackgroundWorker deleteThread;

        /// <summary>Array of the type string</summary>
        string[] availableDocuments;

        /// <summary>Array of the type string</summary>
        string[] availablePictures;

        /// <summary>Array of the type string</summary>
        string[] availableVideos;

        /// <summary>List object of the type string</summary>
        List<string> documentPaths;

        /// <summary>List object of the type string</summary>
        List<string> picturePaths;

        /// <summary>List object of the type string</summary>
        List<string> videoPaths;

        /// <summary>A list of subdirectories found at current directory</summary>
        List<string> subdirectories = new List<string>();

        /// <summary>variabel containing current folder when navigating in lbx_Available</summary>
        private string folderPath = MainWindow.rootFolder;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Variable for the rootfolder, only changed if you change root directory
        /// We are ROOT!
        /// </summary>
        private string iAmROOT = MainWindow.rootFolder;

        /// <summary>Object of the type MainWindow.</summary>
        private MainWindow mainWindow;

        /// <summary>ObservableCollection containing a list of all files that will be added to the database</summary>
        private ObservableCollection<FileList> addNewFiles;

        private QuestionWindow qw;

        private int attempt;

        private SurfaceListBoxItem selectedItem;

        private string selectedItemString;

        private string myFileType;

        private ObservableCollection<FileList> lbxPathOfferings = new ObservableCollection<FileList>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReprogramTags(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            offerings = dc.GetTable<Offering>();
            tags = dc.GetTable<Tag>();
            documents = dc.GetTable<Document>();
            videos = dc.GetTable<Video>();
            images = dc.GetTable<Image>();
            category = dc.GetTable<Category>();
            offeringID = 1;
            parentID = 0;
            parent = null;
            attempt = 0;
            this.categories = DBHelper.GetCategories(offeringID);
            this.categoryIndex = 0;
            this.numberOfCategories = 0;
            btn_CategoryForward.IsEnabled = false;
            btn_CategoryBack.IsEnabled = false;

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            updateCategoryLabel();
            AddToListBox(false);
            AddToCategoryList();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            sqare.Background = brush;
            lbl_CurrentLvl.Content = offeringName;
            canEditColor = (Color)ColorConverter.ConvertFromString("#FF5BD8FF");

            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialogIsShown = false;
            addNewFiles = new ObservableCollection<FileList>();

            currentFileTypeString = "";
            t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 1);
            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(
            OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(
                OnDriveRemoved);
            driveDetector.QueryRemove += new DriveDetectorEventHandler(
                OnQueryRemove);

            
            
            detectDrives();
            

        }

        private void detectDrives()
        {
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);

            foreach (var usb in drives)
            {
                
                createListBoxItem(lbx_Available, usb.RootDirectory.ToString(), null, true, false, false, DarkBlue);
            }
        }

        // Called by DriveDetector when removable device in inserted
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            // e.Drive is the drive letter, e.g. "E:\\"
            // If you want to be notified when drive is being removed (and be
            // able to cancel it),
            // set HookQueryRemove to true
            createListBoxItem(lbx_Available, e.Drive, null, true, false, false, DarkBlue);
            e.HookQueryRemove = true;
        }

        // Called by DriveDetector after removable device has been unplugged
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            // TODO: do clean up here, etc. Letter of the removed drive is in
            // e.Drive;
            for (int n = lbx_Available.Items.Count - 1; n >= 0; --n)
            {
                //string removelistitem = "OBJECT";
                if (lbx_Available.Items[n].ToString().Contains(e.Drive))
                {
                    lbx_Available.Items.RemoveAt(n);
                }
            }
            
            
           
        }

        // Called by DriveDetector when removable drive is about to be removed
        private void OnQueryRemove(object sender, DriveDetectorEventArgs e)
        {
            // Should we allow the drive to be unplugged?
            /*if (MessageBox.Show("Allow remove?", "Query remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.Yes)
                e.Cancel = false;        // Allow removal
            else
                e.Cancel = true;         // Cancel the removal of the device*/
        }

        /// <summary>
        /// The Grid that will be pasted on the main window
        /// </summary>


        public Grid Grid
        {
            get
            {
                return this.grid;
            }
            set
            {
                this.grid = value;
            }
        }

        #region Window Handlers

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Method that calls populateListBox
        /// </summary>
        private void AddToListBox(bool needFocus)
        {
            SurfaceListBoxItem lbi_path = new SurfaceListBoxItem();
            populateListBox(lbi_Offering_Click, lbi_path, lbx_path, "Path", false, needFocus);

        }

        private void AddToCategoryList()
        {
            lbx_Categories.Items.Clear();
            SurfaceListBoxItem lbi_category = new SurfaceListBoxItem();
            populateListBox(null, lbi_category, lbx_Categories, "Category", false, false);

        }




        /// <summary>
        /// Method that updates lbx_contains
        /// </summary>
        private void AddContainingFiles(string filetype)
        {
            createListBoxItem(lbx_Contains, "Files in Database", null, true, true, false, White);
            SurfaceListBoxItem lbi_files = new SurfaceListBoxItem();
            if (currentFileType == FileType.Documents)
            {
                GetFilesByCategory();
            }
            else
            {
                populateListBox(null, lbi_files, lbx_Contains, filetype, true, false);
            }
            int filesFromDB = lbx_Contains.Items.Count;

            if (lbx_Contains.HasItems == true)
            {
                containsEnable(filesFromDB);
            }
            createListBoxItem(lbx_Contains, "New Files", null, true, true, false, White);
            addNewItemsToContain();

        }

        private void GetFilesByCategory()
        {
            List<string> currentCategory = new List<string>();
            currentCategory = DBHelper.GetCategories(offeringID);
            SurfaceListBoxItem lbi_files = new SurfaceListBoxItem();
            if (currentCategory.Count() != 0)
            {
                var first = lbl_CurrentCategory.Content.ToString();
                if (currentCategory.Contains(first))
                {
                    aCategory = first;
                    createListBoxItem(lbx_Contains, first, null, true, true, false, DarkBlue);
                    populateListBox(null, lbi_files, lbx_Contains, "Documents", true, false);
                }

                foreach (var item in currentCategory)
                {

                    if (!item.Equals(first))
                    {
                        aCategory = item;
                        createListBoxItem(lbx_Contains, item, null, true, true, false, DarkBlue);
                        populateListBox(null, lbi_files, lbx_Contains, "Documents", true, false);
                    }

                }
            }
        }

        /// <summary>
        /// Enable/Disable lbx_contaings elements from database depending on chb_delete
        /// </summary>
        private void containsEnable(int filesfromDB)
        {
            bool isChecked = chb_delete.IsChecked.GetValueOrDefault();
            for (int i = 1; i < filesfromDB; i++)
            {
                SurfaceListBoxItem selectedItem = lbx_Contains.Items[i] as SurfaceListBoxItem;
                switch (isChecked)
                {
                    case true:

                        selectedItem.IsEnabled = true;

                        break;

                    case false:

                        selectedItem.IsEnabled = false;

                        break;
                }
            }
        }


        /// <summary>
        /// Dynamic class that get elements from database
        /// Called in PopulateListBox
        /// </summary>
        private dynamic CreateFileList(string switchName)
        {
            try
            {
                switch (switchName)
                {
                    case "Documents":
                        //currentFileType = FileType.Documents;
                        var availableDocuments = documents.Select(data => new { data.Title, data.OfferingName, data.CategoryName, data.OfferingID }).
                            Where(data => data.OfferingID == offeringID && data.CategoryName.Equals(aCategory)
                            ).OrderBy(x => x.Title);

                        return availableDocuments;

                    case "Pictures":
                        //currentFileType = FileType.Pictures;
                        var availableImages = images.Select(data => new { data.Title, data.OfferingName, data.OfferingID }).Where(data => data.OfferingID == offeringID).OrderBy(x => x.Title);

                        return availableImages;
                    case "Videos":
                        //currentFileType = FileType.Videos;
                        var availableVideos = videos.Select(data => new { data.Title, data.OfferingName, data.OfferingID }).Where(data => data.OfferingID == offeringID).OrderBy(x => x.Title);

                        return availableVideos;

                    case "Path":

                        var availablePath = offerings.Select(data => new { data.Parent, data.OfferingName, data.OfferingID, data.ParentID, data.Category }).Where(data => data.ParentID == offeringID).OrderBy(x => x.OfferingName);
                        return availablePath;

                    case "Category":

                        var availableCategories = category.Select(data => new { data.CategoryName, data.CategoryID });
                        if (!tb_CategoryName.Text.Equals(""))
                        {
                            availableCategories = availableCategories.Where(data => data.CategoryName.Contains(tb_CategoryName.Text));
                        }
                        availableCategories = availableCategories.OrderBy(x => x.CategoryName);

                        return availableCategories;
                }
            }
            catch
            {
                DBHelper.RestartService(ref attempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    switch (switchName)
                    {
                        case "Documents":
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Documents);
                            return CreateFileList(switchName);
                        case "Pictures":
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Images);
                            return CreateFileList(switchName);
                        case "Videos":
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                            return CreateFileList(switchName);
                        case "Path":
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                            return CreateFileList(switchName);
                        case "Category":
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                            return CreateFileList(switchName);
                    }
                }
                catch { }
            }
            return null;
        }


        /// <summary>
        /// Adds new items to ObservableCollection addNewFiles
        /// </summary>
        private void addNewItemsToContain()
        {

            if (addNewFiles != null)
            {
                var title = addNewFiles.Select(x => new { x.Title, x.CategoryType, x.FileType, x.OfferingName });

                if (currentFileType == FileType.Documents)
                {
                    title = title.Where(x => x.CategoryType.Equals(lbl_CurrentCategory.Content.ToString()) && x.FileType.Equals(currentFileType.ToString()) && x.OfferingName.Equals(lbl_CurrentLvl.Content.ToString()));
                }
                else
                {
                    title = title.Where(x => x.FileType.Equals(currentFileType.ToString()) && x.OfferingName.Equals(lbl_CurrentLvl.ToString()));
                }

                foreach (var item in title)
                {
                    createListBoxItem(lbx_Contains, item.Title, null, false, false, false, Azure);
                }
            }

        }


        /// <summary>
        /// Populates the listboxItems in lbx_Path and lbx_Contains
        /// </summary>
        private void populateListBox(EventHandler<TouchEventArgs> prevTouchUpEvent, SurfaceListBoxItem lbi_itemName, SurfaceListBox lbx, string switchName, bool file, bool needFocus)
        {
            var availableItems = CreateFileList(switchName);
            try
            {
                foreach (var item in availableItems)
                {
                    lbi_itemName = new SurfaceListBoxItem();
                    if (file == true)
                    {
                        createListBoxItem(lbx, item.Title, null, false, false, false, Azure);
                    }
                    else if (switchName.Equals("Path"))
                    {
                        lbxPathOfferings.Add(new FileList(item.OfferingID, item.OfferingName, item.Parent, item.ParentID));
                        string name = item.OfferingName;
                        int id = item.OfferingID;
                        var hasChildren = offerings.Select(x => new { x.OfferingName, x.ParentID }).Where(x => x.ParentID == id).FirstOrDefault();
                        string content = item.OfferingName;
                        if (hasChildren == null)
                        {
                            createListBoxItem(lbx, content, lbi_Offering_Click, false, false, needFocus, Azure);
                        }
                        else
                        {
                            createListBoxItem(lbx, content, lbi_Offering_Click, true, false, false, DarkBlue);
                        }
                    }
                    else if (switchName.Equals("Category"))
                    {
                        string content = item.CategoryName;
                        //createListBoxItem(lbx, item.CategoryName, null, false, false, false, Azure);
                        createListBoxItem(lbx, content, lbi_Category_Click, false, false, false, Azure);
                    }

                }

            }
            catch (Exception e)
            {

            }
        }


        /// <summary>
        /// Refreshes the lbl_currentCategory Lable when:
        /// chb_allCategories.IsChecked changes
        /// Level changes
        /// all documents in lbx_contains are removed
        /// </summary>
        private void updateCategoryLabel()
        {

            if (chb_allCategories.IsChecked.GetValueOrDefault())
            {
                this.categories = DBHelper.GetAllCategories();
            }
            else
            {

                try
                {
                    this.categories = DBHelper.GetCategories(offeringID);
                }
                catch (Exception ex)
                {
                    try
                    {
                        DBHelper.RestartService(ref attempt);
                        dc = new ABBDataClassesDataContext();
                        dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                    }
                    catch (Exception e)
                    {
                        //if (DBHelper.RestartService(ref restartMssqlServiceAttempt) == 0)
                        using (var sc = new ServiceController("MSSQLSERVER"))
                            MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                        //MainWindow.warningWindow.Show("Unresponsive database service.");
                        //dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                        this.categories = new List<string>();
                    }
                }

            }

            this.numberOfCategories = categories.Count;
            if (numberOfCategories > 0)
            {
                if (currentFileType == FileType.Documents)
                {
                    btn_CategoryForward.IsEnabled = true;
                    btn_CategoryBack.IsEnabled = true;
                    lbx_Available.IsEnabled = true;

                }
                else
                {
                    btn_CategoryForward.IsEnabled = false;
                    btn_CategoryBack.IsEnabled = false;
                    lbx_Available.IsEnabled = true;
                }

                lbl_CurrentCategory.Content = categories.FirstOrDefault();
            }
            else
            {
                if (currentFileType == FileType.Documents)
                {
                    lbx_Available.IsEnabled = false;
                }
                else
                {
                    lbx_Available.IsEnabled = true;
                }
                btn_CategoryForward.IsEnabled = false;
                btn_CategoryBack.IsEnabled = false;
                lbl_CurrentCategory.Content = "";
            }

        }



        /// <summary>
        /// refreshes UI elements
        /// </summary>
        private void refreshContent()
        {
            updateCategoryLabel();
            lbx_path.Items.Clear();
            AddToListBox(false);
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();

            //this.Category = categories.FirstOrDefault();
        }

        /// <summary>
        /// Creates single SurfaceListboxItems with dark blue large font
        /// IsEnabled false in lbx_current. Used as header element
        /// IsEnabled true in lbx_available. Used as folder element
        /// </summary>
        private void createListBoxItem(SurfaceListBox lbx, string content, EventHandler<TouchEventArgs> prevTouchUpEvent, bool isHeader, bool untouchable, bool needFocus, SolidColorBrush color)
        {
            try
            {
                SurfaceListBoxItem lbi_new = new SurfaceListBoxItem();
                lbi_new.Content = content;
                if (isHeader)
                {
                    lbi_new.FontSize = (int)MainWindow.configurationData.FontSize + 4;

                }
                else
                {
                    lbi_new.FontSize = (int)MainWindow.configurationData.FontSize;

                }
                lbi_new.Foreground = color;
                lbi_new.Background = new LinearGradientBrush(
                    new GradientStopCollection {
                            new GradientStop(Color.FromArgb(0xFF,0x3C,0x3C,0x3C), 0.0), 
                            new GradientStop(Color.FromArgb(0xFF,0x02,0x02,0x02), 1.0)
                        },
                    new System.Windows.Point(0.5, 0),
                    new System.Windows.Point(0.5, 1));

                lbi_new.VerticalContentAlignment = VerticalAlignment.Center;

                if (prevTouchUpEvent != null)
                {
                    lbi_new.PreviewTouchUp += prevTouchUpEvent;
                }

                if (untouchable)
                {
                    lbi_new.IsHitTestVisible = false;
                }

                lbx.Items.Add(lbi_new);

                if (needFocus)
                {
                    lbx.ScrollIntoView(lbi_new);
                }
            }
            catch { }
        }



        /// <summary>
        /// Get list of all files of current filetype
        /// </summary>
        private string[] GetEveryFileOfType(FileType fileType, string searchLocation)
        {

            IEnumerable<string> paths = null;
            int numberOfFilesFound = 0;
            switch (fileType)
            {
                case FileType.Documents:

                    paths = SearchAllDirectories(searchLocation,"documents");
                    documentPaths = paths.ToList();
                    numberOfFilesFound = paths.Count();


                    break;
                case FileType.Pictures:


                    paths = SearchAllDirectories(searchLocation,"images");
                    picturePaths = paths.ToList();
                    numberOfFilesFound = paths.Count();

                    break;
                case FileType.Videos:


                    paths = SearchAllDirectories(searchLocation,"videos");
                    videoPaths = paths.ToList();
                    numberOfFilesFound = paths.Count();

                    break;
            }

            string[] fileNamesOfInterest = ExtractNameAndConvertToArray(paths, numberOfFilesFound);
            IOrderedEnumerable<string> fileNamesSorted = fileNamesOfInterest.OrderBy(s => s);

            string[] fileNames = new string[fileNamesOfInterest.Length];
            int counter = 0;
            foreach (string name in fileNamesSorted)
            {
                fileNames[counter] = name;
                counter++;
            }

            return fileNames;

        }


        /// <summary>
        /// Get list of fileNames
        /// Contain some garbage code
        /// </summary>
        private string[] ExtractNameAndConvertToArray(IEnumerable<string> paths, int numberOfFilesFound)
        {
            string[] fileNames = new string[numberOfFilesFound];
            int counter = 0;
            foreach (string path in paths)
            {
                string fileName = ExtractNameFromPath(path);
                if (fileName.Substring(0, 4) != "page")
                {
                    fileNames[counter] = fileName;
                    counter++;
                }
            }

            string[] fileNamesOfInterest = new string[counter];

            for (int i = 0; i < counter; i++)
                fileNamesOfInterest[i] = fileNames[i];

            return fileNamesOfInterest;
        }


        /// <summary>
        /// check if filesnames in lbx_Available matches files saved in database with current category and filetype
        /// </summary>
        private void fileExistInDb()
        {

            foreach (SurfaceListBoxItem item in lbx_Available.Items)
            {

                foreach (SurfaceListBoxItem item2 in lbx_Contains.Items)
                {
                    if (item.Content.ToString().Equals(item2.Content.ToString()) && item2.Foreground != DarkBlue)
                    {
                        item.IsEnabled = false;
                        break;
                    }
                    else
                    {
                        item.IsEnabled = true;
                    }

                }

            }


        }


        /// <summary>
        /// create an array of strings from folderpath
        /// </summary>
        private string ExtractNameFromPath(string path)
        {
            string[] substrings = path.Split('\\');
            return substrings[substrings.Length - 1];
        }



        /// <summary>
        /// search folderPath for all files and populate lbx_Available with current fileType
        /// </summary>
        private void searchDisc()
        {
            string searchLocation = folderPath;

            searchThread = new BackgroundWorker();
            searchThread.RunWorkerAsync();

            searchThread.DoWork += (senderObject, args) =>
            {

                availableDocuments = GetEveryFileOfType(FileType.Documents, searchLocation);
                availablePictures = GetEveryFileOfType(FileType.Pictures, searchLocation);
                availableVideos = GetEveryFileOfType(FileType.Videos, searchLocation);
            };

            searchThread.RunWorkerCompleted += (senderObject, args) =>
            {
                switch (currentFileType)
                {
                    case FileType.Documents:
                        UpdateListBoxes(availableDocuments);
                        break;
                    case FileType.Pictures:
                        UpdateListBoxes(availablePictures);
                        break;
                    case FileType.Videos:
                        UpdateListBoxes(availableVideos);
                        break;
                }
            };
        }


        /// <summary>
        /// search directory from filePath and returns a collection of filenames depending on currentFileType
        /// </summary>
        public IEnumerable<string> SearchAllDirectories(string root, string filetype)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);

            while (pending.Count != 0)
            {
                var path = pending.Pop();
                IEnumerable<string> next = null;

                /*foreach(var item in filetypes){
                    var filteredFiles = Directory
                    .GetFiles(root, "*.*")
                    .Where(file => file.ToLower().EndsWith(item)
                    .ToList();
                 
                 */
                try
                {
                    if (filetype.Equals("videos"))
                        next = Directory.GetFiles(path, "*.*.*.*.*.*").Where(s => s.ToLower().EndsWith(videoFormats[0]) || s.ToLower().EndsWith(videoFormats[1]) || s.ToLower().EndsWith(videoFormats[2]) || s.ToLower().EndsWith(videoFormats[3]) || s.ToLower().EndsWith(videoFormats[4]) || s.ToLower().EndsWith(videoFormats[5]));
                    else if (filetype.Equals("images"))
                        next = Directory.GetFiles(path, "*.*.*.*.*").Where(s => s.ToLower().EndsWith(imageFormats[0]) || s.ToLower().EndsWith(imageFormats[1]) || s.ToLower().EndsWith(imageFormats[2]) || s.ToLower().EndsWith(imageFormats[3]) || s.ToLower().EndsWith(imageFormats[4]));
                    else
                        next = Directory.GetFiles(path, "*").Where(s => s.ToLower().EndsWith(".pdf"));

                }
                catch { }
                if (next != null && next.Count() != 0)
                    foreach (var file in next) yield return file;

            }

        }


        /// <summary>
        /// Populates []subdirectories with all subfolders of folderPath and populates lbx_Available
        /// </summary>
        private void findDirs()
        {


            try
            {
                var dir = Directory.GetDirectories(folderPath);
                foreach (var subdir in dir)
                {

                    var folderName = ExtractNameFromPath(subdir.ToString());
                    subdirectories.Add(folderName);

                };
            }
            catch { }
            try
            {
                foreach (var item in subdirectories)
                    createListBoxItem(lbx_Available, item, lbi_Folder_Click, true, false, false, DarkBlue);
            }
            catch { }
        }

        /// <summary>
        ///Update lbx_Available
        ///Includes folders if show folders are checked
        /// </summary>
        private void UpdateListBoxes(string[] listOfAvailibleFiles)
        {

            lbx_Available.Items.Clear();
            subdirectories.Clear();
            if (enableFolders.IsChecked == true)
            {
                findDirs();
            }

            if (availableDocuments != null)
                foreach (string file in listOfAvailibleFiles)
                {
                    createListBoxItem(lbx_Available, file, null, false, false, false, Azure);
                }
            fileExistInDb();
        }


        /// <summary>
        /// Get list of paths depending on currentFileType
        /// </summary>
        private string GetPath(SurfaceListBoxItem selectedItem)
        {
            string path = null;

            List<string> listOfPaths = null;

            switch (currentFileType)
            {
                case FileType.Documents:
                    listOfPaths = documentPaths;
                    break;
                case FileType.Pictures:
                    listOfPaths = picturePaths;
                    break;
                case FileType.Videos:
                    listOfPaths = videoPaths;
                    break;
            }

            for (int i = 0; i < listOfPaths.Count(); i++)
            {
                if ((string)selectedItem.Content == listOfPaths[i].Split('\\').Last())
                {

                    path = listOfPaths[i];
                    break;
                }

            }

            return path;
        }

        private void goToOffering()
        {
            try
            {
                var goTo = tags.Select(x => new { x.OfferingID, x.TagID }).Where(x => x.TagID == tagValue).FirstOrDefault();
                var query = offerings.Select(x => new { x.OfferingID, x.ParentID }).Where(x => x.OfferingID == goTo.OfferingID).FirstOrDefault();
                offeringID = query.OfferingID;
                parentID = query.ParentID.Value;
            }
            catch { }

            /*
            var offering = offerings.Select(x => new { x.OfferingID }).Where(x => x.OfferingID == offeringID).FirstOrDefault();

            if (offering != null)
            {
                lbx_path.Items.Clear();
                AddToListBox(false);
            }
             */
        }

        /// <summary>
        /// Moves down level if SurfaceListBoxItem in lbx_path is doubleTapped and element has child
        /// </summary>
        private void offeringForward()
        {
            var hasChildren = offerings.Select(x => new { x.OfferingName, x.Parent, x.ParentID }).Where(x => x.ParentID == offeringID).FirstOrDefault();
            
            if (hasChildren != null)
            {
                lbx_path.Items.Clear();
                AddToListBox(false);
            }
        }

        #endregion

        #region Events


        /// <summary>
        /// Updates Canvas square if Tagcard is placed
        /// </summary>
        private void sqare_TouchEnter(object sender, TouchEventArgs e)
        {
            
            bool isTag = e.TouchDevice.GetIsTagRecognized();
            tagValue = (int)e.TouchDevice.GetTagData().Value + 1;

            try
            {
                var canEditTag = tags.Select(x => new { x.IsEditable, x.TagID, x.OfferingName, x.OfferingID }).Where(x => tagValue == x.TagID).FirstOrDefault();

                if (isTag)
                {

                    ++enterCount;

                    if (enterCount == 1)
                    {
                        if (canEditTag.IsEditable.Equals(true))
                        {

                            canEditColor = (Color)ColorConverter.ConvertFromString("#FF5BD8FF");
                            btn_Save.IsEnabled = true;
                            lbl_cantEdit.Content = "SmartCard level: " + canEditTag.OfferingName;
                            lbl_cantEdit.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF5BD8FF"));
                            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
                            System.Windows.Controls.Label bg = lbl_cantEdit;
                            bg.BeginAnimation(Label.OpacityProperty, fadeIn);

                            if (canEditTag.OfferingID != null)
                            {
                                btn_moveToLvl.IsEnabled = true;
                            }

                        }
                        else
                        {
                            canEditColor = (Color)ColorConverter.ConvertFromString("#FFF26760");
                            lbl_cantEdit.Content = "You can not edit this SmartCard";
                            lbl_cantEdit.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF26760"));
                            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
                            System.Windows.Controls.Label bg = lbl_cantEdit;
                            bg.BeginAnimation(Label.OpacityProperty, fadeIn);
                        }

                        ColorAnimation animation = new ColorAnimation(Colors.LightGray, canEditColor, new Duration(TimeSpan.FromSeconds(1)));
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);


                    }

                }
            }
            catch
            {
                DBHelper.RestartService(ref attempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Tags);
                }
                catch (Exception ex) { }
            }


        }


        /// <summary>
        /// Updates Canvas square if Tagcard is removed
        /// </summary>
        private void sqare_TouchLeave(object sender, TouchEventArgs e)
        {
            bool isTag = e.TouchDevice.GetIsTagRecognized();
            if (isTag)
            {
                --enterCount;
                if (enterCount < 1)
                {
                    ColorAnimation animation = new ColorAnimation(canEditColor, Colors.LightGray, new Duration(TimeSpan.FromSeconds(1)));
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                    btn_Save.IsEnabled = false;
                    btn_moveToLvl.IsEnabled = false;
                    DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                    System.Windows.Controls.Label bg = lbl_cantEdit;
                    bg.BeginAnimation(Label.OpacityProperty, fadeOut);

                }


            }
        }

        private void updateIDs()
        {
            foreach (FileList o in lbxPathOfferings)
            {
                if (o.OfferingName.Equals(offeringName))
                {

                    try
                    {
                        var query = offerings.Select(x => new { x.OfferingID, x.ParentID, x.OfferingName, x.Parent }).Where(x => x.OfferingID == o.OfferingID && x.ParentID == o.ParentID).FirstOrDefault();
                        offeringID = query.OfferingID;
                        parentID = query.ParentID.Value;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Event if SurfaceListBoxItem is clicked
        /// Single click updates current level
        /// double click moves down a level
        /// </summary>
        private void lbi_Offering_Click(object sender, TouchEventArgs e)
        {
            SurfaceListBoxItem sb = sender as SurfaceListBoxItem;
            offeringName = sb.Content.ToString();
            updateIDs();
            /*
            foreach (FileList o in lbxPathOfferings)
            {
                if(o.OfferingName.Equals(offeringName)){
                    
                    try
                    {
                        var query = offerings.Select(x => new { x.OfferingID, x.ParentID, x.OfferingName, x.Parent }).Where(x => x.OfferingID == o.OfferingID && x.ParentID == o.ParentID).FirstOrDefault();
                        offeringID = query.OfferingID;
                        parentID = query.ParentID.Value;
                    }
                    catch { }
                }
            }
            */
            if (doubletap == 0)
            {
                t.Tick += t_Tick;
                t.Start();
                doubletap++;
                tappedObject = sender;
            }
            else if (tappedObject == sender)
            {
                offeringForward();
                doubletap = 0;
                t.Tick -= t_Tick;
            }
            else
            {
                tappedObject = null;
                doubletap = 0;
                t.Tick -= t_Tick;
            }

            if (btn_Back.IsEnabled.Equals(false))
            {
                btn_Back.IsEnabled = true;
            }

            this.categories = DBHelper.GetCategories(offeringID);
            this.numberOfCategories = categories.Count;

            lbl_CurrentLvl.Content = sb.Content.ToString();

            updateCategoryLabel();
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();

        }

        private void lbi_Category_Click(object sender, TouchEventArgs e)
        {
            SurfaceListBoxItem sb = sender as SurfaceListBoxItem;
            chb_allCategories.IsChecked = true;
            lbl_CurrentCategory.Content = sb.Content;
        }


        /// <summary>
        /// Timer used to time doubleClick
        /// </summary>
        private void t_Tick(object sender, EventArgs e)
        {
            t.Tick -= t_Tick;
            doubletap = 0;
            tappedObject = null;
        }


        /// <summary>
        /// Moves up a level in lbx_path
        /// </summary>
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            var hasParent = offerings.Select(x => new { x.OfferingName, x.OfferingID, x.Parent, x.ParentID }).Where(x => x.OfferingID == offeringID).FirstOrDefault();

            try
            {
                if (hasParent != null)
                {
                    lbl_CurrentLvl.Content = hasParent.Parent;
                    offeringName = hasParent.Parent;
                    offeringID = hasParent.ParentID.Value;
                    var getParent = offerings.Select(x => new { x.OfferingID, x.ParentID }).Where(x => x.OfferingID == offeringID).FirstOrDefault();
                    if (getParent.ParentID != null)
                        parentID = hasParent.ParentID.Value;
                    else
                        parentID = 0;
                }
                if (offeringName.Equals("Product Guide"))
                {
                    btn_Back.IsEnabled = false;

                }

                try
                {

                    refreshContent();

                }
                catch { }
            }
            catch { }
        }

        /// <summary>
        /// Save Tag card location to database
        /// </summary>
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.UpdateTag(this.offeringName, this.offeringID, tagValue);
            ColorAnimation animation = new ColorAnimation((Color)ColorConverter.ConvertFromString("#FF5BD8FF"), (Color)ColorConverter.ConvertFromString("#4cf59f"), new Duration(TimeSpan.FromSeconds(1)));
            canEditColor = (Color)ColorConverter.ConvertFromString("#4cf59f");
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            lbl_cantEdit.Content = "Location saved!";
            lbl_cantEdit.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4cf59f"));
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            System.Windows.Controls.Label bg = lbl_cantEdit;
            bg.BeginAnimation(Label.OpacityProperty, fadeIn);
        }

        /// <summary>
        /// switches the filetype used
        /// </summary>
        private void btn_FileTypeBack_Click(object sender, RoutedEventArgs e)
        {
            switch (lbl_CurrentFileType.Content.ToString())
            {
                case "Documents":
                    //lbl_CurrentCategory.Content = "";
                    lbl_CurrentFileType.Content = "Videos";
                    currentFileType = FileType.Videos;
                    lbl_CurrentCategory.Opacity = 0.5;
                    btn_CategoryForward.IsEnabled = false;
                    btn_CategoryBack.IsEnabled = false;
                    updateCategoryLabel();
                    UpdateListBoxes(availableVideos);

                    break;
                case "Pictures":
                    lbl_CurrentFileType.Content = "Documents";
                    currentFileType = FileType.Documents;
                    lbl_CurrentCategory.Opacity = 1;
                    //lbl_CurrentCategory.Content = categories.FirstOrDefault();
                    updateCategoryLabel();
                    UpdateListBoxes(availableDocuments);

                    break;
                case "Videos":
                    //lbl_CurrentCategory.Content = "";
                    lbl_CurrentCategory.Opacity = 0.5;
                    lbl_CurrentFileType.Content = "Pictures";
                    currentFileType = FileType.Pictures;
                    UpdateListBoxes(availablePictures);

                    break;
            }

            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }

        /// <summary>
        /// switches the filetype used
        /// </summary>
        private void btn_FileTypeForward_Click(object sender, RoutedEventArgs e)
        {

            switch (lbl_CurrentFileType.Content.ToString())
            {
                case "Documents":
                    //lbl_CurrentCategory.Content = "";
                    lbl_CurrentFileType.Content = "Pictures";
                    currentFileType = FileType.Pictures;
                    btn_CategoryForward.IsEnabled = false;
                    btn_CategoryBack.IsEnabled = false;
                    lbl_CurrentCategory.Opacity = 0.5;
                    updateCategoryLabel();
                    UpdateListBoxes(availablePictures);
                    break;
                case "Pictures":
                    //lbl_CurrentCategory.Content = "";
                    lbl_CurrentFileType.Content = "Videos";
                    currentFileType = FileType.Videos;
                    lbl_CurrentCategory.Opacity = 0.5;
                    UpdateListBoxes(availableVideos);
                    break;
                case "Videos":
                    lbl_CurrentFileType.Content = "Documents";
                    currentFileType = FileType.Documents;
                    lbl_CurrentCategory.Opacity = 1;
                    lbl_CurrentCategory.Content = categories.FirstOrDefault();
                    btn_CategoryForward.IsEnabled = true;
                    btn_CategoryBack.IsEnabled = true;
                    updateCategoryLabel();
                    UpdateListBoxes(availableDocuments);
                    break;
            }

            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }

        /// <summary>
        /// switches the category used
        /// </summary>
        private void btn_CategoryBack_Click(object sender, RoutedEventArgs e)
        {
            if (categoryIndex == 0)
            {
                categoryIndex = this.numberOfCategories - 1;
            }
            else
            {
                categoryIndex--;
            }
            try
            {
                lbl_CurrentCategory.Content = categories.ElementAt(categoryIndex);
            }
            catch { }
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }


        /// <summary>
        /// switches the category used
        /// </summary>
        private void btn_CategoryForward_Click(object sender, RoutedEventArgs e)
        {
            if (categoryIndex == this.numberOfCategories - 1)
            {
                categoryIndex = 0;
            }
            else
            {
                categoryIndex++;
            }
            try
            {
                lbl_CurrentCategory.Content = categories.ElementAt(categoryIndex);
            }
            catch { }
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }

        /// <summary>
        /// Disable SurfaceListBoxItem in lbx_Available
        /// Makes copy to lbx_contains under add file
        /// Adds selected item to addNewFiles
        /// </summary>
        private void btn_MoveToDb_Click(object sender, RoutedEventArgs e)
        {
            SurfaceListBoxItem selectedItem = lbx_Available.SelectedItem as SurfaceListBoxItem;


            string category = null;

            if (selectedItem != null && selectedItem.Foreground != DarkBlue)
            {
                if (lbl_CurrentCategory != null)
                {
                    if (lbl_CurrentCategory.IsEnabled == true)
                    {
                        category = lbl_CurrentCategory.Content.ToString();
                    }

                    string filepath = GetPath(selectedItem);
                    addNewFiles.Add(new FileList(currentFileType.ToString(), lbl_CurrentLvl.Content.ToString(), selectedItem.Content.ToString(), filepath, category));


                    createListBoxItem(lbx_Contains, selectedItem.Content.ToString(), null, false, false, true, Azure);
                    fileExistInDb();
                    if (!btn_SaveFiles.IsEnabled)
                    {
                        btn_SaveFiles.IsEnabled = true;
                    }

                    if (!btn_cancelFiles.IsEnabled)
                    {
                        btn_cancelFiles.IsEnabled = true;
                    }

                    selectedItem.IsSelected = false;



                }
            }
        }

        /// <summary>
        /// If double tapped navigates to subfolder in lbx_Available
        /// </summary>
        private void lbi_Folder_Click(object sender, TouchEventArgs e)
        {



            if (doubletap == 0)
            {
                t.Tick += t_Tick;
                t.Start();
                doubletap++;
                tappedObject = sender;
            }
            else if (tappedObject == sender)
            {
                btn_FolderBack.IsEnabled = true;
                SurfaceListBoxItem selectedItem = sender as SurfaceListBoxItem;
                folderPath = folderPath + "\\" + selectedItem.Content.ToString();
                findDirs();
                searchDisc();
                doubletap = 0;
                t.Tick -= t_Tick;
            }
            else
            {
                tappedObject = null;
                doubletap = 0;
                t.Tick -= t_Tick;
            }

        }

        /// <summary>
        /// Save new offering to database
        /// </summary>
        private void btn_SaveOffering_Click(object sender, RoutedEventArgs e)
        {
            Offering offering = new Offering();
            offering.OfferingName = this.tb_OfferingName.Text;
            this.tb_OfferingName.Text = "";
            offering.Parent = this.offeringName;
            offering.ParentID = this.offeringID;
            DBHelper.InsertOffering(offering);
            lbx_path.Items.Clear();
            AddToListBox(true);
            lbx_path.ScrollIntoView(lbx_path.Items.IndexOf(offering.OfferingName));
            if (!offeringName.Equals("Product Guide"))
            {
                btn_Back.IsEnabled = true;
            }

        }

        /// <summary>
        /// Save new cateory to database
        /// </summary>
        private void btn_saveCategory_Click(object sender, RoutedEventArgs e)
        {
            Category category = new Category();
            category.CategoryName = this.tb_CategoryName.Text;
            this.tb_CategoryName.Text = "";
            DBHelper.insertCategory(category);
            updateCategoryLabel();
            AddToCategoryList();
            //refresh av fremtidig tabell
        }


        /// <summary>
        /// update listbox depending on status of chb_delete is checked/unchecked
        /// </summary>
        private void chb_delete_Checked(object sender, RoutedEventArgs e)
        {
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }

        /// <summary>
        /// Opens folder navigation
        /// Changes IamROOT
        /// </summary>
        private void btn_ChangeDir_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Surface.SurfaceKeyboard.IsVisible = true;
            FolderDialog.Description = "USE MOUSE OR TOUCH KEYBOARD TO NAVIGATE\n↑, ↓ : Browse list\n←, → : Open/Close directory";
            FolderDialog.ShowNewFolderButton = false;
            if (!folderDialogIsShown)
            {
                folderDialogIsShown = true;
                System.Windows.Forms.DialogResult dialogRes = FolderDialog.ShowDialog();
                if (dialogRes == System.Windows.Forms.DialogResult.OK)
                {
                    iAmROOT = FolderDialog.SelectedPath;
                    folderPath = iAmROOT;
                    folderDialogIsShown = false;
                    searchDisc();
                }
                else
                {
                    folderDialogIsShown = false;
                }

            }

        }

        private void enableDisableElements(bool x)
        {
            lbx_Available.IsEnabled = x;
            lbx_Contains.IsEnabled = x;
            lbx_path.IsEnabled = x;
            sqare.IsEnabled = x;
            //tb_OfferingName.IsEnabled = x;
            //tb_CategoryName.IsEnabled = x;
            chb_allCategories.IsEnabled = x;
            chb_delete.IsEnabled = x;
            enableFolders.IsEnabled = x;
            //btn_cancelFiles.IsEnabled = x;
            btn_Cancel1.IsEnabled = x;
            btn_Cancel2.IsEnabled = x;
            btn_ChangeDir.IsEnabled = x;
            btn_CategoryForward.IsEnabled = x;
            btn_CategoryBack.IsEnabled = x;
            btn_FileTypeForward.IsEnabled = x;
            btn_FileTypeBack.IsEnabled = x;
            btn_MoveToDb.IsEnabled = x;
            btn_DeleteFromDb.IsEnabled = x;
            //btn_Back.IsEnabled = x;
            //btn_Save.IsEnabled = x;
            btn_Exit.IsEnabled = x;
            //btn_FolderBack.IsEnabled = x;
        }

        /// <summary>
        /// saves all files from addNewFiles to database
        /// </summary>
        private void btn_SaveFiles_Click(object sender, RoutedEventArgs e)
        {
            loadingAnimation.Visibility = Visibility.Visible;
            enableDisableElements(false);
            createThread = new BackgroundWorker();
            createThread.RunWorkerAsync();

            createThread.DoWork += (senderObject, args) =>
            {
                if (DBHelper.InsertFiles(addNewFiles) == false)
                {
                    using (var sc = new ServiceController("MSSQLSERVER"))
                        MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                }
            };

            createThread.RunWorkerCompleted += (senderObject, args) =>
            {
                enableDisableElements(true);
                addNewFiles.Clear();
                lbx_Contains.Items.Clear();
                updateCategoryLabel();
                AddContainingFiles(lbl_CurrentFileType.Content.ToString());
                fileExistInDb();

                loadingAnimation.Visibility = Visibility.Collapsed;
            };
        }

        /// <summary>
        /// enable/disable add new offering 
        /// </summary>
        private void btn_Cancel1_Click(object sender, RoutedEventArgs e)
        {
            switch (btn_Cancel1.Content.ToString())
            {
                case "New":
                    btn_Cancel1.Content = "Cancel";
                    tb_OfferingName.IsEnabled = true;
                    btn_SaveOffering.IsEnabled = true;

                    break;

                case "Cancel":
                    btn_Cancel1.Content = "New";
                    btn_SaveOffering.IsEnabled = false;
                    tb_OfferingName.IsEnabled = false;
                    tb_OfferingName.Clear();

                    break;
            }
        }

        /// <summary>
        /// enable/disable add new category
        /// </summary>
        private void btn_Cancel2_Click(object sender, RoutedEventArgs e)
        {
            switch (btn_Cancel2.Content.ToString())
            {
                case "New":
                    btn_Cancel2.Content = "Cancel";
                    tb_CategoryName.IsEnabled = true;
                    btn_saveCategory.IsEnabled = true;

                    break;

                case "Cancel":
                    btn_Cancel2.Content = "New";
                    btn_saveCategory.IsEnabled = false;
                    tb_CategoryName.IsEnabled = false;
                    tb_CategoryName.Clear();

                    break;
            }
        }

        /// <summary>
        /// removes all elements from addNewFiles and disables btn_saveFiles and btn_cancel
        /// </summary>
        private void btn_cancelFiles_Click(object sender, RoutedEventArgs e)
        {
            addNewFiles.Clear();
            btn_cancelFiles.IsEnabled = false;
            btn_SaveFiles.IsEnabled = false;
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
        }

        /// <summary>
        /// updates lbl_Currentcategory depending on status
        /// </summary>
        private void chb_allCategories_Checked(object sender, RoutedEventArgs e)
        {
            updateCategoryLabel();
            if (chb_allCategories.IsChecked == true)
            {
                lbx_Available.IsEnabled = true;

            }

        }

        /// <summary>
        /// navigates to parant folder if folderpath != iAmROOT
        /// </summary>
        private void btn_FolderBack_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                int index = folderPath.LastIndexOf('\\') + 1;
                int slash = folderPath.LastIndexOf('\\');

                folderPath = folderPath.Remove(index);
                folderPath = folderPath.Remove(slash);
            }
            catch (Exception ex)
            {
                MainWindow.warningWindow.Show("HORY SHET WHAT HAPPENDEDED O.0");
            }
            findDirs();
            searchDisc();
            if (folderPath.Equals(iAmROOT))
            {
                btn_FolderBack.IsEnabled = false;
            }
        }

        /// <summary>
        /// Show/hide folders from lbx_Available
        /// </summary>
        private void enableFolders_Checked(object sender, RoutedEventArgs e)
        {
            if (enableFolders.IsChecked == true)
            {
                findDirs();
            }

            searchDisc();
        }

        /// <summary>
        /// Delete single item from database
        /// removes item from addnewfiles
        /// </summary>
        private void btn_DeleteFromDb_Click(object sender, RoutedEventArgs e)
        {

            selectedItem = lbx_Contains.SelectedItem as SurfaceListBoxItem;
            if (selectedItem != null)
            {
                selectedItemString = selectedItem.Content.ToString();
                myFileType = lbl_CurrentFileType.Content.ToString();
                loadingAnimation.Visibility = Visibility.Visible;
                enableDisableElements(false);
                deleteThread = new BackgroundWorker();
                deleteThread.RunWorkerAsync();

                deleteThread.DoWork += (senderObject, args) =>
                {

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        var dontDelete = addNewFiles.FirstOrDefault(x => x.Title.Equals(selectedItem.Content.ToString()));

                        if (dontDelete != null)
                        {

                            lbx_Contains.Items.Remove(selectedItem);
                            addNewFiles.Remove(dontDelete);
                            fileExistInDb();


                        }
                        else if (DBHelper.DeleteFile(selectedItemString, myFileType) == false)
                        {
                            using (var sc = new ServiceController("MSSQLSERVER"))
                                MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                        }

                    }));

                };
                deleteThread.RunWorkerCompleted += (senderObject, args) =>
                {
                    if (addNewFiles.Count == 0)
                    {
                        if (btn_SaveFiles.IsEnabled)
                        {
                            btn_SaveFiles.IsEnabled = false;
                        }

                        if (btn_cancelFiles.IsEnabled)
                        {
                            btn_cancelFiles.IsEnabled = false;
                        }
                    }
                    enableDisableElements(true);
                    lbx_Contains.Items.Clear();
                    AddContainingFiles(lbl_CurrentFileType.Content.ToString());
                    fileExistInDb();
                    //refreshContent();
                    loadingAnimation.Visibility = Visibility.Collapsed;
                };

            }
        }


        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (addNewFiles.Count > 0)
            {
                qw = new QuestionWindow(mainWindow);
                qw.Show("Are you sure you want to quit?");
            }
            else
            {
                mainWindow.RefreshContent();
                mainWindow.mainGrid.Children.Remove(mainWindow.grid);
                mainWindow.mainGrid.AllowDrop = true;
                mainWindow.RefreshContent();
                mainWindow.ToolBarGrid.Visibility = Visibility.Visible;

                mainWindow.BlurEffect.Radius = 0;
                mainWindow.BlurEffectBack.Radius = 0;
                mainWindow.ScatterWindow.Visibility = Visibility.Visible;
                /*
                mainWindow.RefreshContent();
                mainWindow.mainGrid.Children.Remove(mainWindow.grid);
                mainWindow.RefreshContent();
                
                mainWindow.RefreshContent();
                */
            }
        }

        public void answerQuestion(QuestionWindow questionWindow, MessageBoxResult mbr)
        {
            switch (mbr)
            {
                case MessageBoxResult.Yes:
                    mainWindow.RefreshContent();
                    mainWindow.mainGrid.Children.Remove(mainWindow.grid);
                    mainWindow.RefreshContent();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        #endregion

        private void tb_CategoryName_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddToCategoryList();
        }

        private void btn_moveToLvl_Click(object sender, RoutedEventArgs e)
        {
            string[] label = lbl_cantEdit.Content.ToString().Split(':');
            string lvl = label.Last();
            lvl = lvl.Substring(1);
            offeringName = lvl;
            goToOffering();
            if (!offeringName.Equals("Product Guide") && btn_Back.IsEnabled.Equals(false))
            {
                btn_Back.IsEnabled = true;
            }
            this.categories = DBHelper.GetCategories(offeringID);
            this.numberOfCategories = categories.Count;
            lbl_CurrentLvl.Content = lvl;
            updateCategoryLabel();
            lbx_Contains.Items.Clear();
            AddContainingFiles(lbl_CurrentFileType.Content.ToString());
            fileExistInDb();
            refreshContent();
        }
    }


}