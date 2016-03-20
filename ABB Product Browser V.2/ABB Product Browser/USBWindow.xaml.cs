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
using Dolinay;
using System.Windows.Media.Animation;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Resources;
using System.ComponentModel;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class USBWindow : SurfaceWindow
    {
        private string[] videoFormats = { ".avi", ".wmv", ".mov", ".mp4", ".h.264", ".avchd" };
        private string[] imageFormats = { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };

        List<string> documentPaths;

        /// <summary>List object of the type string</summary>
        List<string> picturePaths;

        /// <summary>List object of the type string</summary>
        List<string> videoPaths;

        private string path;
        private string IAMROOT = null;



        Color Azure = Color.FromArgb(0xFF, 0x5B, 0xD8, 0xFF);
        Color Yellow = Color.FromArgb(0xFF, 0xF4, 0xEE, 0x7F);
        SolidColorBrush brush = new SolidColorBrush(Colors.Red);
        DriveDetector driveDetector;
        ObservableCollection<FileList> usbList = new ObservableCollection<FileList>();
        ObservableCollection<FileList> buttonList = new ObservableCollection<FileList>();
        private BackgroundWorker buttonThread;
        int deg = 0;
        private System.Windows.Threading.DispatcherTimer loadingTimer;
        int radius = 0;
        bool buttonOut = false;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public USBWindow()
        {
            InitializeComponent();
            driveDetector = new DriveDetector();


            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            Ellipse_loading.Fill = brush;
            Ellipse_Normal.Fill = brush;
            detectDrives();
            loadingTimer = new System.Windows.Threading.DispatcherTimer();
            loadingTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            loadingTimer.Tick += t_TickLoading;
            documentPaths = new List<string>();
            picturePaths = new List<string>();
            videoPaths = new List<string>();

        }
        #region TagWindow Events
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
            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            driveDetector.QueryRemove += new DriveDetectorEventHandler(OnQueryRemove);
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
            driveDetector.DeviceArrived -= new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved -= new DriveDetectorEventHandler(OnDriveRemoved);
            driveDetector.QueryRemove -= new DriveDetectorEventHandler(OnQueryRemove);
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

        #region usbEvents


        // Called by DriveDetector when removable device in inserted
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            // e.Drive is the drive letter, e.g. "E:\\"
            // If you want to be notified when drive is being removed (and be
            // able to cancel it),
            // set HookQueryRemove to true
            if (usbList.Count == 0)
                changeColor(Colors.Red, Azure, 0.5);

            

            if (IAMROOT == null)
            {
                addNewDrives(e.Drive);
                moveFunc(buttonList.Last(), false);
            }
               

            //buttonList.Last().ButtonGrid += btn_USB_PreviewTouchUp;
            e.HookQueryRemove = true;
            //usbLoading();
        }

        // Called by DriveDetector after removable device has been unplugged
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            // TODO: do clean up here, etc. Letter of the removed drive is in
            // e.Drive;

            if (e.Drive.Equals(IAMROOT))
            {
                foreach (var item in buttonList)
                {

                    if (item.ShowButton)
                        moveFunc(item, true);
                    else
                        removebutton(item);
                }

                buttonList.Clear();
            }



            for (int n = usbList.Count - 1; n >= 0; --n)
            {
                //string removelistitem = "OBJECT";
                if (usbList[n].UsbRoot.ToString().Contains(e.Drive))
                {
                    if (buttonList.Count != 0)
                    {
                        if (buttonList[n].Index == usbList[n].Index)
                        {
                            moveFunc(buttonList[n], true);
                            buttonList.Remove(buttonList[n]);
                        }
                    }
                    usbList.RemoveAt(n);
                }
            }

            if (e.Drive.Equals(IAMROOT))
            {
                usbList.Clear();
                detectDrives();
            }

            if (usbList.Count == 0)
                changeColor(brush.Color, Colors.Red, 0.5);
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

        #endregion

        private void addNewDrives(string root)
        {

            int counter = 0;
            bool freeSlot = false;
            var usb = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable && drive.Name.Equals(root)).FirstOrDefault();

            while (!freeSlot)
            {
                if (buttonList.Count <= counter || buttonList[counter].Index != counter)
                {
                    //counter++;
                    usbList.Add(new FileList(usb.VolumeLabel, usb.RootDirectory.ToString(), counter));
                    createUsbButtons(usb.RootDirectory.ToString(), usb.VolumeLabel, counter, btn_USB_PreviewTouchUp);
                    freeSlot = true;
                }
                else
                    counter++;
            }

        }

        private void detectDrives()
        {
            //usbList.Clear();
            int counter = 0;
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);

            foreach (var usb in drives)
            {

                usbList.Add(new FileList(usb.VolumeLabel, usb.RootDirectory.ToString(), counter));
                createUsbButtons(usb.RootDirectory.ToString(), usb.VolumeLabel, counter, btn_USB_PreviewTouchUp);
                moveFunc(buttonList[counter], false);
                counter++;

            }

            if (usbList.Count != 0)
                changeColor(brush.Color, Azure, 0.5);
        }

        private void changeColor(Color start, Color stop, double duration)
        {
            ColorAnimation animation = new ColorAnimation(start, stop, new Duration(TimeSpan.FromSeconds(duration)));
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void usbLoading()
        {
            Loading.Visibility = Visibility.Visible;
            RotateTransform angle = new RotateTransform();
            angle.Angle = deg;
            angle.CenterX = Ellipse_wheel.ActualHeight / 2;
            angle.CenterY = Ellipse_wheel.ActualWidth / 2;

            Ellipse_wheel.RenderTransform = angle;

            deg += 8;
            if (deg >= 360)
                deg = 0;

        }


        public Grid USBGrid()
        {

            return USB_Grid;
        }

        private void Ellipse_PreviewTouchUp(object sender, TouchEventArgs e)
        {



        }


        private void btn_USB_PreviewTouchUp(object sender, TouchEventArgs e)
        {

            var activeDrive = sender as SurfaceButton;
            //var count = buttonList.Count;
           /* for (int i = buttonList.Count - 1; i >= 0; i--)
            {
                if (buttonList[i].ShowButton)
                    moveFunc(buttonList[i], true);
                else
                    removebutton(buttonList[i]);
            }*/
            
            foreach (var item in buttonList)
            {
                if (item.ShowButton)
                    moveFunc(item, true);
                else
                    removebutton(item);
            }
            buttonList.Clear();


            /* System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
             timer.Interval = new TimeSpan(0, 0, 0, 1);
             timer.Start();
             timer.Tick += (s, args) =>
             {*/
            changeColor(brush.Color, Yellow, 0.5);
            loadingTimer.Start();
            BackgroundWorker fileThread = new BackgroundWorker();
            fileThread.RunWorkerAsync();
            path = activeDrive.Content.ToString();
            IAMROOT = activeDrive.Content.ToString();

            fileThread.DoWork += (senderObjects, arg) =>
            {


                SearchDrive(path, "documents", false);
                SearchDrive(path, "videos", false);
                SearchDrive(path, "images", false);
                loadingTimer.Stop();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    Loading.Visibility = Visibility.Collapsed;
                    changeColor(brush.Color, Colors.Green, 0.5);
                }));

            };
            fileThread.RunWorkerCompleted += (senderObject, arg) =>
                {

                    createUsbButtons(null, "Videos", 7, btn_Videos_PreviewTouchUp);
                    createUsbButtons(null, "Pictures", 0, btn_Pictures_PreviewTouchUp);
                    createUsbButtons(null, "Documents", 1, btn_Documents_PreviewTouchUp);
                    createUsbButtons(null, "More", 3, btn_Switch_PreviewTouchUp);

                    foreach (var item in buttonList)
                    {
                        moveFunc(item, false);
                    }

                    changeColor(brush.Color, Azure, 0.5);

                    createUsbButtons(null, "Return", 5, btn_Return_PreviewTouchUp);
                    createUsbButtons(null, "Folders", 2, btn_Return_PreviewTouchUp);
                    createUsbButtons(null, "Back", 6, btn_Switch_PreviewTouchUp);
                    createUsbButtons(null, "Eject", 3, btn_Return_PreviewTouchUp);
                    createUsbButtons(null, "Scan Drive", 4, btn_ScanDrive_PreviewTouchUp);
                    createUsbButtons(null, "Download", 1, btn_ScanDrive_PreviewTouchUp);

                };



        }

        private void btn_Return_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            
            /*for (int i = buttonList.Count-1; i >= 0; i--)
            {
                if (buttonList[i].ShowButton)
                    moveFunc(buttonList[i], true);
                else
                    removebutton(buttonList[i]);
            }*/
            foreach (var item in buttonList)
            {

                if (item.ShowButton)
                    moveFunc(item, true);
                else
                    removebutton(item);
            }
            buttonList.Clear();
            IAMROOT = null;
            usbList.Clear();
            detectDrives();
        
        }

        private void btn_Pictures_PreviewTouchUp(object sender, TouchEventArgs e)
        { }

        private void btn_Videos_PreviewTouchUp(object sender, TouchEventArgs e)
        { }

        private void btn_Documents_PreviewTouchUp(object sender, TouchEventArgs e)
        {

        }

        private void btn_ScanDrive_PreviewTouchUp(object sender, TouchEventArgs e)
        {

            changeColor(brush.Color, Yellow, 0.5);
            loadingTimer.Start();
            BackgroundWorker fileThread = new BackgroundWorker();
            fileThread.RunWorkerAsync();

            fileThread.DoWork += (senderObjects, arg) =>
            {

                SearchDrive(path, "documents", true);
                SearchDrive(path, "videos", true);
                SearchDrive(path, "images", true);
                loadingTimer.Stop();
                this.Dispatcher.Invoke((Action)(() =>
                   {
                       Loading.Visibility = Visibility.Collapsed;
                       changeColor(brush.Color, Colors.Green, 0.5);
                   }));
            };
            fileThread.RunWorkerCompleted += (senderObject, args) =>
            {
                changeColor(brush.Color, Azure, 0.5);
            };
        }

        private void btn_Switch_PreviewTouchUp(object sender, TouchEventArgs e)
        {

            foreach (var item in buttonList)
            {
                moveFunc(item, false);
            }


        }


        private void t_TickLoading(object sender, EventArgs e)
        {
            Loading.Visibility = Visibility.Visible;
            RotateTransform angle = new RotateTransform();
            angle.Angle = deg;
            angle.CenterX = Ellipse_wheel.ActualHeight / 2;
            angle.CenterY = Ellipse_wheel.ActualWidth / 2;

            Ellipse_wheel.RenderTransform = angle;

            deg += 8;
            if (deg >= 360)
                deg = 0;

        }

        #region  Buttons

        private void moveFunc(FileList btnGrid, bool remove)
        {


            System.Windows.Threading.DispatcherTimer buttonsTimer = new System.Windows.Threading.DispatcherTimer();
            buttonsTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            buttonsTimer.Start();
            buttonsTimer.Tick += (s, args) =>
            {
                var angle = (Math.PI * (btnGrid.Radius / 1.4444)) / 180;
                var speed = Math.Sin(angle);
                moveSingleButton(btnGrid);

                if (!btnGrid.ShowButton)
                {

                    btnGrid.Radius += 14 * speed;

                    if (btnGrid.Radius >= 248)
                    {
                        btnGrid.Radius = 254;
                        btnGrid.ShowButton = true;
                        buttonsTimer.Stop();
                    }
                }
                else
                {
                    btnGrid.Radius -= 14 * speed;

                    if (btnGrid.Radius <= 12)
                    {

                        btnGrid.Radius = 12;

                        if (remove)
                        {
                            removebutton(btnGrid);
                        }
                        else
                        {
                            btnGrid.ShowButton = false;
                        }

                        buttonsTimer.Stop();

                    }
                }


            };

        }


        private void removebutton(FileList buttonGrid)
        {

            USB_Grid.Children.Remove(buttonGrid.ButtonGrid);
            //buttonList.Remove(buttonGrid);

        }

        private void moveSingleButton(FileList btnGrid)
        {

            double angle = (Math.PI * (btnGrid.Index * 45)) / 180;
            double posX = ((Math.Sin(angle) * btnGrid.Radius));
            double posY = ((Math.Cos(angle) * btnGrid.Radius) + 25);

            btnGrid.ButtonGrid.Margin = new Thickness(posX, posY, 0, 0);

        }

        private void createUsbButtons(string root, string name, int index, EventHandler<TouchEventArgs> prevTouchUpEvent)
        {
            Grid button_Grid = new Grid();

            button_Grid.Height = 94;
            button_Grid.HorizontalAlignment = HorizontalAlignment.Center;
            button_Grid.VerticalAlignment = VerticalAlignment.Center;
            button_Grid.Margin = new Thickness(0, 25, 0, 0);

            Ellipse background = new Ellipse();
            background.Fill = brush;
            background.Width = 55;
            background.Height = 55;
            background.Margin = new Thickness(0, 4, 0, 0);
            background.HorizontalAlignment = HorizontalAlignment.Center;
            background.VerticalAlignment = VerticalAlignment.Top;

            string resourcePath = getbuttonImage(name);

            SurfaceButton btn_Usb = new SurfaceButton();
            StreamResourceInfo buttonStreamInfo = Application.GetResourceStream(new Uri(resourcePath, UriKind.Relative));
            BitmapFrame ButtonImage = BitmapFrame.Create(buttonStreamInfo.Stream);
            ImageBrush buttonImgBrush = new ImageBrush();
            buttonImgBrush.ImageSource = ButtonImage;
            btn_Usb.Background = buttonImgBrush;
            btn_Usb.VerticalAlignment = VerticalAlignment.Top;
            btn_Usb.HorizontalAlignment = HorizontalAlignment.Center;
            btn_Usb.Height = 60;
            btn_Usb.Width = 60;
            btn_Usb.Content = root;
            btn_Usb.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn_Usb.VerticalContentAlignment = VerticalAlignment.Center;
            btn_Usb.Foreground = brush;
            btn_Usb.PreviewTouchUp += prevTouchUpEvent;

            Label lbl_name = new Label();
            lbl_name.Content = name;
            lbl_name.HorizontalAlignment = HorizontalAlignment.Center;
            lbl_name.VerticalAlignment = VerticalAlignment.Bottom;
            lbl_name.FontSize = 14;
            lbl_name.Foreground = brush;
            //lbl_name.Visibility = Visibility.Hidden;

            button_Grid.Children.Add(background);
            button_Grid.Children.Add(btn_Usb);
            button_Grid.Children.Add(lbl_name);
            Panel.SetZIndex(button_Grid, index);
            buttonList.Add(new FileList(button_Grid, 12, index, false));
            Panel.SetZIndex(main_grid, 10);
            USB_Grid.Children.Add(buttonList.Last().ButtonGrid);

        }


        private string getbuttonImage(string name)
        {
            string resourcePath = "Resources/USB Window/USB_GUI_Button.png";
            switch (name)
            {
                case "Videos":
                    resourcePath = "Resources/USB Window/Video.png";
                    break;

                case "Documents":
                    resourcePath = "Resources/USB Window/Documents.png";
                    break;

                case "Pictures":
                    resourcePath = "Resources/USB Window/Picture.png";
                    break;

                case "Return":
                    resourcePath = "Resources/USB Window/Return.png";
                    break;

                case "Eject":
                    resourcePath = "Resources/USB Window/Eject.png";
                    break;
                case "Forward":
                    resourcePath = "Resources/USB Window/Forward.png";
                    break;

                case "Back":
                    resourcePath = "Resources/USB Window/Back.png";
                    break;

                case "Folders":
                    resourcePath = "Resources/USB Window/Folder.png";
                    break;

                case "Scan Drive":
                    resourcePath = "Resources/USB Window/Scan Drive.png";
                    break;
                case "More":
                    resourcePath = "Resources/USB Window/More.png";
                    break;
                case "Download":
                    resourcePath = "Resources/USB Window/Download.png";
                    break;
            }


            return resourcePath;
        }
        #endregion


        private void SearchDrive(string root, string filetype, bool scanDrive)
        {

            Stack<string> pending = new Stack<string>();
            pending.Push(root);


            while (pending.Count != 0)
            {
                int counter = 0;
                var path = pending.Pop();
                IEnumerable<string> next = null;

                try
                {
                    if (filetype.Equals("videos"))
                    {
                        next = Directory.GetFiles(path, "*.*.*.*.*.*").Where(s => s.ToLower().EndsWith(videoFormats[0]) || s.ToLower().EndsWith(videoFormats[1]) || s.ToLower().EndsWith(videoFormats[2]) || s.ToLower().EndsWith(videoFormats[3]) || s.ToLower().EndsWith(videoFormats[4]) || s.ToLower().EndsWith(videoFormats[5]));
                        if (next != null && next.Count() != 0)
                        {

                            foreach (var file in next)
                            {
                                videoPaths.Add(file);
                                counter++;
                            }
                        }

                    }
                    else if (filetype.Equals("images"))
                    {
                        next = Directory.GetFiles(path, "*.*.*.*.*").Where(s => s.ToLower().EndsWith(imageFormats[0]) || s.ToLower().EndsWith(imageFormats[1]) || s.ToLower().EndsWith(imageFormats[2]) || s.ToLower().EndsWith(imageFormats[3]) || s.ToLower().EndsWith(imageFormats[4]));
                        if (next != null && next.Count() != 0)
                        {

                            foreach (var file in next)
                            {
                                picturePaths.Add(file);
                                counter++;
                            }
                        }
                    }
                    else
                    {
                        next = Directory.GetFiles(path, "*").Where(s => s.ToLower().EndsWith(".pdf"));

                        if (next != null && next.Count() != 0)
                        {

                            foreach (var file in next)
                            {
                                documentPaths.Add(file);
                                counter++;
                            }
                        }
                    }
                }
                catch { }

                if (scanDrive)
                {

                    try
                    {
                        next = Directory.GetDirectories(path);
                        foreach (var subdir in next) pending.Push(subdir);
                    }
                    catch { }

                }

            }

        }

    }



}