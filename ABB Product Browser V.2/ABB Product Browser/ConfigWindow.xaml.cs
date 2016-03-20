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
using System.IO;
using System.Diagnostics;
using System.Timers;
using System.Threading;
using System.Windows.Media.Animation;
using SysImages = System.Windows.Controls.Image;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : TagVisualization
    {
        /// <summary>Variable of the type Timer.</summary>
        private System.Timers.Timer t;
        /// <summary>Constant of the type double containing default video size.</summary>
        const double defaultVideoSize = 4;
        /// <summary>Constant of the type double containing default DPI for jpeg convertion.</summary>
        const double defaultDPI = 100;
        /// <summary>Constant of the type double containing default font size.</summary>
        const double defaultFontSize = 12;
        /// <summary>Constant of the type double containing default lost tag timeout.</summary>
        const double defaultLostTagTimeout = 2;
        /// <summary>Constant of the type double containing default orientation offset from tag.</summary>
        const double defaultOrientationOffsetFromTag = 270;
        /// <summary>Constant of the type double containing default container box color frame thickness.</summary>
        const double defaultBoxColorFrameThickness = 2;
        /// <summary>Constant of the type double containing default card color frame thickness.</summary>
        const double defaultCardColorFrameThickness = 5;
        /// <summary>Constant of the type double containing default close button size.</summary>
        const double defaultCloseButtonSize = 25;
        /// <summary>
        /// Variable of the type FolderBrowserDialog.
        /// </summary>
        private System.Windows.Forms.FolderBrowserDialog FolderDialog;
        /// <summary>
        /// Variable of the type Boolean.
        /// </summary>
        private bool folderDialogIsShown;
        /// <summary>
        /// Constructor initializes the graphics and functionality of the config window.
        /// </summary>
        /// 
        public MainWindow mainWindow;

        public ConfigWindow()
        {
            InitializeComponent();
            setLabels();
            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialogIsShown = false;
        }
        /// <summary>
        /// Method that runs after config window is finished loading.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: customize ConfigWindow's UI based on this.VisualizedTag here
         
            foreach (StringData sd in MainWindow.configData)
            {
                switch (sd.stringTag)
                {
                    case "RootFolder":
                        txt_RootFolder.Text = sd.stringValue;
                        break;
                    case "Language":
                        if (sd.stringValue.ToLower() == "english")
                        {
                            rdb_English.IsChecked = true;
                        }
                        else if (sd.stringValue.ToLower() == "norsk")
                        {
                            rdb_Norsk.IsChecked = true;
                        }
                        else
                            rdb_English.IsChecked = true;
                        break;
                    case "FontSize":
                        double fontSizeValue = Convert.ToDouble(sd.stringValue);
                        if (fontSizeValue < 8 || fontSizeValue > 20)
                            fontSizeValue = defaultFontSize;
                        sld_FontSize.Value = fontSizeValue;
                        lbl_FontSizeValue.Content = sld_FontSize.Value.ToString() + " pt";
                        break;
                    case "DPI":
                        double DPIValue = Convert.ToDouble(sd.stringValue);
                        if (DPIValue < 50 || DPIValue > 300)
                            DPIValue = defaultDPI;
                        sld_DPI.Value = DPIValue;
                        lbl_DPISliderValue.Content = sld_DPI.Value.ToString();
                        break;
                    case "VideoInitialSize":
                        double videoSizeValue = Convert.ToDouble(sd.stringValue);
                        if (videoSizeValue < 1 || videoSizeValue > 5)
                            videoSizeValue = defaultVideoSize;
                        sld_VideoSize.Value = videoSizeValue;
                        lbl_VideoSizeSliderValue.Content = "1/" + Convert.ToInt32(sld_VideoSize.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("screen")).stringValue;
                        break;
                    case "LostTagTimeout":
                        double lostTagTimeoutValue = Convert.ToDouble(sd.stringValue) / 1000;
                        if (lostTagTimeoutValue < 0 || lostTagTimeoutValue > 5)
                            lostTagTimeoutValue = defaultLostTagTimeout;
                        sld_LostTagTimeout.Value = lostTagTimeoutValue;
                        lbl_LostTagTimeoutSliderValue.Content = Convert.ToInt32(sld_LostTagTimeout.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("sec")).stringValue;
                        break;
                    case "OrientationOffsetFromTag":
                        double OrientationOffsetFromTagValue = Convert.ToDouble(sd.stringValue);
                        if (OrientationOffsetFromTagValue < 0 || OrientationOffsetFromTagValue > 360)
                            OrientationOffsetFromTagValue = defaultOrientationOffsetFromTag;
                        sld_OrientationOffsetFromTag.Value = OrientationOffsetFromTagValue;
                        lbl_OrientationOffsetFromTagValue.Content = Convert.ToInt32(sld_OrientationOffsetFromTag.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("degrees")).stringValue;
                        break;
                    case "BoxColorFrameThickness":
                        double boxColorFrameThicknessValue = Convert.ToDouble(sd.stringValue);
                        if (boxColorFrameThicknessValue < 0 || boxColorFrameThicknessValue > 10)
                            boxColorFrameThicknessValue = defaultBoxColorFrameThickness;
                        sld_BoxColorFrameThickness.Value = boxColorFrameThicknessValue;
                        lbl_BoxColorFrameThicknessValue.Content = Convert.ToInt32(sld_BoxColorFrameThickness.Value).ToString() + " pt";
                        break;
                    case "CardColorFrameThickness":
                        double cardColorFrameThicknessValue = Convert.ToDouble(sd.stringValue);
                        if (cardColorFrameThicknessValue < 0 || cardColorFrameThicknessValue > 10)
                            cardColorFrameThicknessValue = defaultCardColorFrameThickness;
                        sld_CardColorFrameThickness.Value = cardColorFrameThicknessValue;
                        lbl_CardColorFrameThicknessValue.Content = Convert.ToInt32(sld_CardColorFrameThickness.Value).ToString() + " pt";
                        break;
                    case "CloseButtonSize":
                        double closeButtonSizeValue = Convert.ToDouble(sd.stringValue);
                        if (closeButtonSizeValue < 15 || closeButtonSizeValue > 40)
                            closeButtonSizeValue = defaultCloseButtonSize;
                        sld_CloseButtonSize.Value = closeButtonSizeValue;
                        lbl_CloseButtonSizeValue.Content = Convert.ToInt32(sld_CloseButtonSize.Value).ToString() + " pt";
                        break;
                    case "DoubleTap":
                        if (sd.stringValue == "true")
                            chb_DoubleTap.IsChecked = true;
                        else
                            chb_DoubleTap.IsChecked = false;
                        break;
                }
            }
        }

        public void SetMainWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        /// <summary>
        /// Method that updates values in the config file, Config.txt.
        /// </summary>
        private void updateConfigFile()
        {
            StreamWriter newConfigFile = new StreamWriter("new.txt");
            newConfigFile.WriteLine("RootFolder;" + txt_RootFolder.Text);
            string language = "English";
            if (rdb_English.IsChecked == true)
                language = "English";
            else if (rdb_Norsk.IsChecked == true)
                language = "Norsk";
            newConfigFile.WriteLine("Language;" + language);
            newConfigFile.WriteLine();
            newConfigFile.WriteLine("FontSize;" + Convert.ToInt32(sld_FontSize.Value).ToString());
            newConfigFile.WriteLine("DPI;" + Convert.ToInt32(sld_DPI.Value).ToString());
            newConfigFile.WriteLine("VideoInitialSize;" + Convert.ToInt32(sld_VideoSize.Value).ToString());
            newConfigFile.WriteLine("LostTagTimeout;" + (Convert.ToInt32(sld_LostTagTimeout.Value) * 1000).ToString());
            newConfigFile.WriteLine("OrientationOffsetFromTag;" + Convert.ToInt32(sld_OrientationOffsetFromTag.Value).ToString());
            newConfigFile.WriteLine();
            newConfigFile.WriteLine("BoxColorFrameThickness;" + Convert.ToInt32(sld_BoxColorFrameThickness.Value).ToString());
            newConfigFile.WriteLine("CardColorFrameThickness;" + Convert.ToInt32(sld_CardColorFrameThickness.Value).ToString());
            newConfigFile.WriteLine();
            newConfigFile.WriteLine("CloseButtonSize;" + Convert.ToInt32(sld_CloseButtonSize.Value).ToString());
            newConfigFile.WriteLine();
            string doubleTap;
            if (chb_DoubleTap.IsChecked == true)
                doubleTap = "true";
            else
                doubleTap = "false";
            newConfigFile.WriteLine("DoubleTap;" + doubleTap);
            newConfigFile.Close();
            File.Replace("new.txt", "Config.txt", "BackupFromOldConfig.txt");
        }
        /// <summary>
        /// Method that checks norsk radio button and unchecks english radio button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void rdb_Norsk_Checked(object sender, RoutedEventArgs e)
        {
            rdb_English.IsChecked = false;
        }
        /// <summary>
        /// Method that checks english radio button and unchecks norsk radio button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void rdb_English_Checked(object sender, RoutedEventArgs e)
        {
            rdb_Norsk.IsChecked = false;
        }

        /// <summary>
        /// Method that runs when save button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            fadeOut.AutoReverse = true;
            //SysImages bg = btn_Save.Background;
            btn_Save.BeginAnimation(SysImages.OpacityProperty, fadeOut);
            t = new System.Timers.Timer();
            t.Interval = 3000;
            t.Enabled = true;
            lbl_saved.Visibility = Visibility.Visible;
            t.Start();
            t.Elapsed += HideSavedLable;
            updateConfigFile();
            updateConfigSettings();
        }

        /// <summary>
        /// Method that updates the config settings array in main window.
        /// </summary>
        private void updateConfigSettings()
        {
            
            //MainWindow.ReadConfigFile();
            MainWindow.configData = DBHelper.ReadConfigSettings();
            MainWindow.ReadLanguageFile(MainWindow.applicationLanguage + ".txt");
            setLabels();
        }

        /// <summary>
        /// Method that runs when DPI slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_DPI_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_DPI.Value >= sld_DPI.Minimum && sld_DPI.Value <= sld_DPI.Maximum && lbl_DPISliderValue != null)
                lbl_DPISliderValue.Content = Convert.ToInt32(sld_DPI.Value).ToString();
        }

        /// <summary>
        /// Method that runs when VideoSize slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_VideoSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_VideoSize.Value >= sld_VideoSize.Minimum && sld_VideoSize.Value <= sld_VideoSize.Maximum && lbl_VideoSizeSliderValue != null)
                lbl_VideoSizeSliderValue.Content = "1/" + Convert.ToInt32(sld_VideoSize.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("screen")).stringValue;
        }

        /// <summary>
        /// Method that runs when Lost tag timeout slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_LostTagTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_LostTagTimeout.Value >= sld_LostTagTimeout.Minimum && sld_LostTagTimeout.Value <= sld_LostTagTimeout.Maximum && lbl_LostTagTimeoutSliderValue != null)
                lbl_LostTagTimeoutSliderValue.Content = Convert.ToInt32(sld_LostTagTimeout.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("sec")).stringValue;
        }

        /// <summary>
        /// Method that runs when Orientation offset from tag slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_OrientationOffsetFromTag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_OrientationOffsetFromTag.Value >= sld_OrientationOffsetFromTag.Minimum && sld_OrientationOffsetFromTag.Value <= sld_OrientationOffsetFromTag.Maximum && lbl_OrientationOffsetFromTagValue != null)
                lbl_OrientationOffsetFromTagValue.Content = Convert.ToInt32(sld_OrientationOffsetFromTag.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("degrees")).stringValue;
        }

        /// <summary>
        /// Method that runs when box color frame thickness slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_BoxColorFrameThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_BoxColorFrameThickness.Value >= sld_BoxColorFrameThickness.Minimum && sld_BoxColorFrameThickness.Value <= sld_BoxColorFrameThickness.Maximum && lbl_BoxColorFrameThicknessValue != null)
                lbl_BoxColorFrameThicknessValue.Content = Convert.ToInt32(sld_BoxColorFrameThickness.Value).ToString() + " pt";
        }

        /// <summary>
        /// Method that runs when card color frame thickness slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_CardColorFrameThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_CardColorFrameThickness.Value >= sld_CardColorFrameThickness.Minimum && sld_CardColorFrameThickness.Value <= sld_CardColorFrameThickness.Maximum && lbl_CardColorFrameThicknessValue != null)
                lbl_CardColorFrameThicknessValue.Content = Convert.ToInt32(sld_CardColorFrameThickness.Value).ToString() + " pt";
        }

        /// <summary>
        /// Method that runs when font size slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_FontSize.Value >= sld_FontSize.Minimum && sld_FontSize.Value <= sld_FontSize.Maximum && lbl_FontSizeValue != null)
                lbl_FontSizeValue.Content = Convert.ToInt32(sld_FontSize.Value).ToString() + " pt";
        }

        /// <summary>
        /// Method that runs when close button size slider value is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void sld_CloseButtonSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_CloseButtonSize.Value >= sld_CloseButtonSize.Minimum && sld_CloseButtonSize.Value <= sld_CloseButtonSize.Maximum && lbl_CloseButtonSizeValue != null)
                lbl_CloseButtonSizeValue.Content = Convert.ToInt32(sld_CloseButtonSize.Value).ToString() + " pt";
        }

        /// <summary>
        /// Method that runs when root folder text field is touched and released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void txt_RootFolder_PreviewTouchUp(object sender, TouchEventArgs e)
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
                txt_RootFolder.Text = folderPath;
        }

        /// <summary>
        /// Method that runs when tag files path text field is touched and released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void txt_TagFilesPath_PreviewTouchUp(object sender, TouchEventArgs e)
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
        }

        /// <summary>
        /// Method that runs when default settings button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void btn_DefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            rdb_English.IsChecked = true;
            rdb_Norsk.IsChecked = false;
            lbl_FontSizeValue.Content = defaultFontSize + " pt";
            sld_FontSize.Value = defaultFontSize;
            lbl_DPISliderValue.Content = defaultDPI;
            sld_DPI.Value = defaultDPI;
            lbl_VideoSizeSliderValue.Content = "1/" + defaultVideoSize + " screen";
            sld_VideoSize.Value = defaultVideoSize;
            lbl_LostTagTimeoutSliderValue.Content = defaultLostTagTimeout + " sec";
            sld_LostTagTimeout.Value = defaultLostTagTimeout;
            lbl_OrientationOffsetFromTagValue.Content = defaultOrientationOffsetFromTag + " degrees";
            sld_OrientationOffsetFromTag.Value = defaultOrientationOffsetFromTag;
            lbl_BoxColorFrameThicknessValue.Content = defaultBoxColorFrameThickness + " pt";
            sld_BoxColorFrameThickness.Value = defaultBoxColorFrameThickness;
            lbl_CardColorFrameThicknessValue.Content = defaultCardColorFrameThickness + " pt";
            sld_CardColorFrameThickness.Value = defaultCardColorFrameThickness;
            lbl_CloseButtonSizeValue.Content = defaultCloseButtonSize + " pt";
            sld_CloseButtonSize.Value = defaultCloseButtonSize;
            chb_DoubleTap.IsChecked = false;
        }

        /// <summary>
        /// Method that hides the saved response lable after timer runs out.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void HideSavedLable(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() => lbl_saved.Visibility = Visibility.Hidden));
            t.Stop();
        }

        /// <summary>
        /// Method that runs when root folder text field is clicked by mouse.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void txt_RootFolder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
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
                txt_RootFolder.Text = folderPath;
        }

        /// <summary>
        /// Method that runs when tag files path text field is clicked by mouse.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void txt_TagFilesPath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderDialog.ShowNewFolderButton = false;
            if (!folderDialogIsShown)
            {
                folderDialogIsShown = true;
                System.Windows.Forms.DialogResult dialogRes = FolderDialog.ShowDialog();
                if (dialogRes == System.Windows.Forms.DialogResult.Cancel || dialogRes == System.Windows.Forms.DialogResult.OK)
                    folderDialogIsShown = false;
            }
        }
        /// <summary>
        /// Method sets the label names.
        /// </summary>
        private void setLabels()
        {
            lbl_BoxColorFrameThickness.Content = MainWindow.languange.Find(x => x.stringTag.Contains("boxcolor")).stringValue;
            lbl_CardColorFrameThickness.Content = MainWindow.languange.Find(x => x.stringTag.Contains("cardcolor")).stringValue;
            lbl_CloseButtonSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("closebutton")).stringValue;
            lbl_DPI.Content = MainWindow.languange.Find(x => x.stringTag.Contains("dpi")).stringValue;
            lbl_LostTagTimeout.Content = MainWindow.languange.Find(x => x.stringTag.Contains("timeout")).stringValue;
            lbl_RootFolder.Content = MainWindow.languange.Find(x => x.stringTag.Contains("rootfolder")).stringValue;
            lbl_VideoInitialSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("videosize")).stringValue;
            lbl_OrientationOffsetFromTag.Content = MainWindow.languange.Find(x => x.stringTag.Contains("offset")).stringValue;
            lbl_Language.Content = MainWindow.languange.Find(x => x.stringTag.Contains("language")).stringValue;
            lbl_FontSize.Content = MainWindow.languange.Find(x => x.stringTag.Contains("fontsize")).stringValue;
            lbl_saved.Content = MainWindow.languange.Find(x => x.stringTag.Contains("saved")).stringValue;
            chb_DoubleTap.Content = MainWindow.languange.Find(x => x.stringTag.Contains("doubletap")).stringValue;
            lbl_VideoSizeSliderValue.Content = "1/" + Convert.ToInt32(sld_VideoSize.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("screen")).stringValue;
            lbl_LostTagTimeoutSliderValue.Content = Convert.ToInt32(sld_LostTagTimeout.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("sec")).stringValue;
            lbl_OrientationOffsetFromTagValue.Content = Convert.ToInt32(sld_OrientationOffsetFromTag.Value).ToString() + " " + MainWindow.languange.Find(x => x.stringTag.Contains("degrees")).stringValue;
        }

        public void TagVisualization_LostTag(object sender, RoutedEventArgs e)
        {
            mainWindow.BlurEffect.Radius = 0;
            mainWindow.BlurEffectBack.Radius = 0;
        }

    }
}
