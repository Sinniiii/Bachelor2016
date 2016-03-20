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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Data.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Threading;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for Configurations.xaml
    /// </summary>
    public partial class Configurations : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// 
         ABBDataClassesDataContext dc = new ABBDataClassesDataContext();
         MainWindowViewModel vm;
         Table <Setting> settings;
         Table<Background> backgrounds;
         Setting configData;
         private System.Windows.Forms.FolderBrowserDialog FolderDialog;
         private bool folderDialogIsShown;
         private List<string> bgPaths = new List<string>();
         private MainWindow mainWindow;

         public Configurations(MainWindow mainWindow)
         {
             this.mainWindow = mainWindow;
             vm = new MainWindowViewModel();
             this.DataContext = vm;
            settings = dc.GetTable<Setting>();
            backgrounds = dc.GetTable<Background>();
            configData = settings.Where(x => x.SettingName.Equals("Current")).FirstOrDefault();
            InitializeComponent();
            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialogIsShown = false;
            
            //setLabels();
            
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            #region AddPreviewDropHandler
            SurfaceDragDrop.AddPreviewDropHandler(MainLibraryAvailable, OnPreviewDropAvailable);
            SurfaceDragDrop.AddPreviewDropHandler(MainLibraryContains, OnPreviewDropContains);
            SurfaceDragDrop.AddPreviewQueryTargetHandler(MainLibraryAvailable, OnPreviewQueryTargetAvailable);
            #endregion

            setLibraryAvailable(configData.RootFolder);
            setLibraryContains();
            Load_Configs("Current"); 
        }



        private void setLibraryAvailable(string path)
        {
            lbl_path.Content = path;
            try
            {
                // Get the list of files.
                //string[] files = System.IO.Directory.GetFiles(path, "*.jpg, *png");
                var filteredFiles = Directory
                .GetFiles(path, "*.*")
                .Where(file => file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("png"))
                .ToList();
                // Create an ObservableCollection, and add the file names.
                // LibraryBar.ItemsSource should implement INotifyCollectionChanged
                // in order for the built-in drag-and-drop capability to work properly.
                ObservableCollection<FileList> items = new ObservableCollection<FileList>();

                foreach (string file in filteredFiles)
                {
                    BitmapImage myBitmapImage = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
                    items.Add(new FileList(myBitmapImage, file));
                }

                // Set the ItemsSource property.
                MainLibraryAvailable.ItemsSource = items;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                // Handle exception as needed.
            }
        }

        private void setLibraryContains()
        {
            //string path = @"C:\Users\Public\Pictures\Sample Pictures";

            try
            {

                var images = backgrounds.Select(x => x.BackgroundBinary);
                // Get the list of files.
                /*var filteredFiles = Directory
                .GetFiles(path, "*.*")
                .Where(file => file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("png"))
                .ToList();*/
                // Create an ObservableCollection, and add the file names.
                // LibraryBar.ItemsSource should implement INotifyCollectionChanged
                // in order for the built-in drag-and-drop capability to work properly.
                //ObservableCollection<string> items = new ObservableCollection<string>();
                ObservableCollection<FileList> items = new ObservableCollection<FileList>();
                //ICollectionView objects = CollectionViewSource.GetDefaultView(items);
                
                //ObservableCollection<FileList> documentList = BlobConverter.addDocumentsByOfferingName(offering, tagWindow, mainWindow);

                foreach (var file in images)
                {
                    BitmapSource img = BlobConverter.ConvertImgBlobtoImage(file);
                    items.Add(new FileList(img, null));
                }

                // Set the ItemsSource property.
                MainLibraryContains.ItemsSource = items;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                // Handle exception as needed.
            }

        }

        private void Load_Configs(string settingName)
        {
            //TODO: customize ConfigWindow's UI based on this.VisualizedTag here
            
            configData = settings.Where(x => x.SettingName.Equals(settingName)).FirstOrDefault();
            
            txt_RootFolder.Text = configData.RootFolder;

            if(configData.Language.ToLower().Equals("english"))
                rdb_English.IsChecked = true;
            else
                rdb_Norsk.IsChecked = true;

            sld_FontSize.Value = (double)configData.FontSize;
            lbl_FontSizeValue.Content = configData.FontSize + "pt";

            sld_shTimer.Value = (double)configData.BgTimer;
            lbl_shTimer.Content = configData.BgTimer + " min";

            sld_LostTagTimeout.Value = (double)configData.LostTagTimeout;
            lbl_LostTagTimeoutSliderValue.Content = configData.LostTagTimeout + " sec";

            sld_OrientationOffsetFromTag.Value = (double)configData.OrientationOffsetFromTag;
            lbl_OrientationOffsetFromTagValue.Content = (configData.OrientationOffsetFromTag * 90) + ('\u00B0').ToString();

            sld_BoxColorFrameThickness.Value = (double)configData.BoxColorFrameThickness;
            lbl_BoxColorFrameThicknessValue.Content = configData.BoxColorFrameThickness + "pt";

            sld_CardColorFrameThickness.Value = (double)configData.CardColorFrameThickness;
            lbl_CardColorFrameThicknessValue.Content = configData.CardColorFrameThickness + "pt";

            sld_CloseButtonSize.Value = (double)configData.CloseButtonSize;
            lbl_CloseButtonSizeValue.Content = configData.CloseButtonSize + "pt";

            chb_DoubleTap.IsChecked = configData.DoubleTap;


        }


        private void setLabels()
        {
            lbl_BoxColorFrameThickness.Content = MainWindow.languange.Find(x => x.stringTag.Contains("boxcolor")).stringValue;
            lbl_CardColorFrameThickness.Content = MainWindow.languange.Find(x => x.stringTag.Contains("cardcolor")).stringValue;
            lbl_CloseButtonSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("closebutton")).stringValue;
            lbl_DPI.Content = MainWindow.languange.Find(x => x.stringTag.Contains("dpi")).stringValue;
            lbl_LostTagTimeout.Content = MainWindow.languange.Find(x => x.stringTag.Contains("timeout")).stringValue;
            //lbl_RootFolder.Content = MainWindow.languange.Find(x => x.stringTag.Contains("rootfolder")).stringValue;
            lbl_VideoInitialSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("videosize")).stringValue;
            lbl_OrientationOffsetFromTag.Content = MainWindow.languange.Find(x => x.stringTag.Contains("offset")).stringValue;
            //lbl_Language.Content = MainWindow.languange.Find(x => x.stringTag.Contains("language")).stringValue;
            lbl_FontSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("fontsize")).stringValue;
            lbl_saved.Content = MainWindow.languange.Find(x => x.stringTag.Contains("saved")).stringValue;
            chb_DoubleTap.Content = MainWindow.languange.Find(x => x.stringTag.Contains("doubletap")).stringValue;
            lbl_VideoSizeSliderValue.Content = "1/" + Convert.ToInt32(sld_VideoSize.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("screen")).stringValue;
            lbl_LostTagTimeoutSliderValue.Content = Convert.ToInt32(sld_LostTagTimeout.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("sec")).stringValue;
            lbl_OrientationOffsetFromTagValue.Content = Convert.ToInt32(sld_OrientationOffsetFromTag.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("degrees")).stringValue;
            /*
            var currentResourceDictionary = (from d in
                                                 BaseModel.Instance.ImportCatalog.ResourceDictionaryList
                                             where d.Metadata.ContainsKey("Culture")
                                             && d.Metadata["Culture"].ToString().Equals(vm.SelectedLanguage.Code)
                                             select d).FirstOrDefault();
            if (currentResourceDictionary != null)
            {
                var previousResourceDictionary = (from d in BaseModel.Instance.ImportCatalog.ResourceDictionaryList
                                                  where d.Metadata.ContainsKey("Culture")
                                                  && d.Metadata["Culture"].ToString().Equals(vm.PreviousLanguage.Code)
                                                  select d).FirstOrDefault();
                if (previousResourceDictionary != null && previousResourceDictionary != currentResourceDictionary)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(previousResourceDictionary.Value);
                    Application.Current.Resources.MergedDictionaries.Add(currentResourceDictionary.Value);
                    CultureInfo cultureInfo = new CultureInfo(vm.SelectedLanguage.Code);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    Application.Current.MainWindow.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

                    vm.PreviousLanguage = vm.SelectedLanguage;
                }
            }
            */
        }


        private void saveConfigurations() { 
        
        
        }




        private void OnPreviewDropAvailable(object sender, SurfaceDragDropEventArgs e)
        {LibraryBar bar = (LibraryBar)sender;
          
            if (bar.Items.Contains(e.Cursor.Data))
            {
               
                e.Effects = DragDropEffects.Move;
                try
                {
                    bgPaths.Remove(e.Cursor.Data.ToString());
                }
                catch { }
            }
        }


        private void OnPreviewDropContains(object sender, SurfaceDragDropEventArgs e)
        {
            LibraryBar bar = (LibraryBar)sender;
            FileList data = e.Cursor.Data as FileList;
            if (!bar.Items.Contains(e.Cursor.Data))
            {

                bgPaths.Add(data.FilePath);

            }
        }

        


       private void OnPreviewQueryTargetAvailable(object sender, QueryTargetEventArgs e)
        {
            LibraryBar bar = (LibraryBar)sender;
            if (!bar.Items.Contains(e.Cursor.Data))
            {
                e.UseDefault = false;
                e.ProposedTarget = MainLibraryContains;
                e.Handled = true;
                //MainWindow.warningWindow.Show(e.OriginalSource.ToString());
                

            }
            else
            {
                e.UseDefault = false;
                e.ProposedTarget = MainLibraryAvailable;
                e.Handled = true;
                //MainWindow.warningWindow.Show(e.OriginalSource.ToString());
            }
       
        }

        private void OnPreviewQueryTargetContains(object sender, QueryTargetEventArgs e)
        {
            LibraryBar bar = (LibraryBar)sender;
            if (!bar.Items.Contains(e.Cursor.Data))
            {
                e.UseDefault = false;
                e.ProposedTarget = MainLibraryContains;
                e.Handled = true;
                //MainWindow.warningWindow.Show(e.OriginalSource.ToString());

            }
            else
            {
                e.UseDefault = false;
                //e.ProposedTarget = MainLibraryAvailable;
                e.Handled = true;
                //MainWindow.warningWindow.Show(e.OriginalSource.ToString());
            }
               
           
        }

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

        private void btn_DefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            Load_Configs("Default");
        }

        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            
            mainWindow.mainGrid.Children.Remove(mainWindow.configGrid);
            mainWindow.RefreshContent();
            mainWindow.ScatterWindow.Visibility = Visibility.Visible;
            mainWindow.mainGrid.AllowDrop = true;
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.InsertSetting(configData);
            DBHelper.InsertBackgrounds(bgPaths);
            bgPaths.Clear();
            setLibraryContains();   
        }

        private void sld_OrientationOffsetFromTag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbl_OrientationOffsetFromTagValue.Content = ((int)e.NewValue * 90) + ('\u00B0').ToString();
            configData.OrientationOffsetFromTag = ((int)e.NewValue * 90);
        }

        private void sld_CardColorFrameThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbl_CardColorFrameThicknessValue.Content = ((int)e.NewValue) + "pt";
            configData.CardColorFrameThickness = (int)e.NewValue;
        }

        private void rdb_Language_Checked(object sender, RoutedEventArgs e)
        {
            SurfaceRadioButton rb = sender as SurfaceRadioButton;
            configData.Language = rb.Content.ToString();
            
            if (rb.Content.ToString().Equals("Norsk"))
            {
                vm.SelectedLanguage = vm.LanguageList.Where(i => i.Name.Equals("Norwegian")).FirstOrDefault();
            }
            else if (rb.Content.ToString().Equals("English"))
            {
                vm.SelectedLanguage = vm.LanguageList.Where(i => i.Name.Equals("English")).FirstOrDefault();
            }
            
            var currentResourceDictionary = (from d in
                                                BaseModel.Instance.ImportCatalog.ResourceDictionaryList
                                            where d.Metadata.ContainsKey("Culture")
                                            && d.Metadata["Culture"].ToString().Equals(vm.SelectedLanguage.Code)
                                            select d).FirstOrDefault();
            if (currentResourceDictionary != null)
            {
                var previousResourceDictionary = (from d in BaseModel.Instance.ImportCatalog.ResourceDictionaryList
                                                  where d.Metadata.ContainsKey("Culture")
                                                  && d.Metadata["Culture"].ToString().Equals(vm.PreviousLanguage.Code)
                                                  select d).FirstOrDefault();
                if (previousResourceDictionary != null && previousResourceDictionary != currentResourceDictionary)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(previousResourceDictionary.Value);
                    Application.Current.Resources.MergedDictionaries.Add(currentResourceDictionary.Value);
                    CultureInfo cultureInfo = new CultureInfo(vm.SelectedLanguage.Code);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    Application.Current.MainWindow.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

                    vm.PreviousLanguage = vm.SelectedLanguage;
                }
            }
            
        }


       private void sld_FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
           lbl_FontSizeValue.Content = ((int)e.NewValue) + "pt";
           configData.FontSize = (int)e.NewValue;
       }

       private void sld_shTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
           lbl_shTimer.Content = ((int)e.NewValue) + " min";
           configData.BgTimer = (int)e.NewValue;
       }

       private void sld_LostTagTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
           lbl_LostTagTimeoutSliderValue.Content = ((int)e.NewValue) + " sec";
           configData.CardColorFrameThickness = (int)e.NewValue;
       }

       private void sld_BoxColorFrameThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
           lbl_BoxColorFrameThicknessValue.Content = ((int)e.NewValue) + "pt";
           configData.BoxColorFrameThickness = (int)e.NewValue;
       }

       private void sld_CloseButtonSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
           lbl_CloseButtonSizeValue.Content = ((int)e.NewValue) + "pt";
           configData.CloseButtonSize = (int)e.NewValue;
       }

       private void txt_RootFolder_PreviewTouchUp(object sender, TouchEventArgs e)
       {

           string folderPath = FolderWindow();
           txt_RootFolder.Text = folderPath;
           configData.RootFolder = folderPath;
       }

       private void txt_RootFolder_TextChanged(object sender, TextChangedEventArgs e)
       {
           configData.RootFolder = txt_RootFolder.Text;
       }

       private void chb_DoubleTap_Checked(object sender, RoutedEventArgs e)
       {
           SurfaceCheckBox cb = sender as SurfaceCheckBox;
           configData.DoubleTap = cb.IsChecked;
       }



       private string FolderWindow()
       {
           Microsoft.Surface.SurfaceKeyboard.IsVisible = true;
           FolderDialog.ShowNewFolderButton = false;
           if (!folderDialogIsShown)
           {
               folderDialogIsShown = true;
               System.Windows.Forms.DialogResult dialogRes = FolderDialog.ShowDialog();
               if (dialogRes == System.Windows.Forms.DialogResult.Cancel || dialogRes == System.Windows.Forms.DialogResult.OK)
                   folderDialogIsShown = false;
           }
           string folderPath = FolderDialog.SelectedPath;
           if (folderPath != "" && folderPath != null)
               return folderPath;
           else return configData.RootFolder;

       }

       private void btn_ChangeDir_Click(object sender, RoutedEventArgs e)
       {
           string folderPath = FolderWindow();
           setLibraryAvailable(folderPath);

       }

    }
}