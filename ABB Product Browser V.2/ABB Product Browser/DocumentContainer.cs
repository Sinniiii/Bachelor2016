using System;
using System.Collections.Generic;
using System.Linq;
using SysImages = System.Windows.Controls.Image;
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
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Resources;
using System.Threading;
using pdftron.PDF;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Data.Linq;
using System.Data;
using System.Reflection;

namespace ProductBrowser
{
    /// <summary>
    /// Logic for the ScatterViewItem DocumentContainer.
    /// </summary>
    public class DocumentContainer : ScatterViewItem
    {
        /// <summary>Variable of the type string containing a document name.</summary>
        private string documentName;
        /// <summary>Variable of the type int containing a pageIndex.</summary>
        private int pageIndex = 0;
        /// <summary>Object of the type Grid.</summary>
        private Grid documentGrid;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_pageIndex;
        /// <summary>Object of the type Label.</summary>
        private Label lbl_productName;
        /// <summary>Object of the type SurfaceSlider.</summary>
        private SurfaceSlider pageSlider;
        /// <summary>Object of the type BackgroundWorker.</summary>
        private BackgroundWorker convertThread;
        /// <summary>List with objects of the type BitmapImage.</summary>
        private List<BitmapImage> listOfPages;
        /// <summary>
        /// List with ScatterViewItem objects.
        /// </summary>
        private List<ScatterViewItem> listOfSinglePages;
        /// <summary>Object of the type Image.</summary>
        private SysImages pageControl;
        /// <summary>Objects of the type Border.</summary>
        private Border colourFrame;
        /// <summary>Object of the type Stopwatch.</summary>
        private readonly Stopwatch doubleTapStopwatch = new Stopwatch();
        /// <summary>Object of the type Point.</summary>
        private System.Windows.Point lastTapLocation;
        /// <summary>Object of the type TagWindow.</summary>
        private TagWindow tagWindow;
        /// <summary>Object of the type MainWindow.</summary>
        private MainWindow mainWindow;
        /// <summary>Object of the type Size.</summary>
        private Size naturalSize;
        /// <summary>
        /// Object of the type BitmapImage.
        /// </summary>
        private BitmapImage bitImage;
        /// <summary>
        /// Object of the type byte[].
        /// </summary>
        private byte[] pdfBinary;
        /// <summary>
        /// LINQ datacontext object.
        /// </summary>
        private ABBDataClassesDataContext dc;
        /// <summary>
        /// Table with Document objects.
        /// </summary>
        Table<Document> documents;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="docId">The document ID. Database property DocumentID.</param>
        /// <param name="title">The document title of your choice.</param>
        /// <param name="tagWindow">The TagWindow bound to this DocumentContainer.</param>
        /// <param name="mainWindow">The MainWindow bound to this DocumentContainer.</param>
        public DocumentContainer(int docId, string title, TagWindow tagWindow, MainWindow mainWindow)
        {
            this.dc = new ABBDataClassesDataContext();
            documents = dc.GetTable<Document>();
            this.tagWindow = tagWindow;
            this.mainWindow = mainWindow;
            this.bitImage = new BitmapImage();
            this.documentName = title;
            InitializeData();
            this.Loaded += DocumentContainer_Loaded;
            this.PreviewTouchUp += documentContainer_Clicked;
            this.ContainerManipulationCompleted += Container_ManipulationCompleted;
            setPdfByteArray(docId);
            DrawProductNameLabel();
            CreatePageSlider();
            DrawIndexLabel();
            CreateDocumentFrame(tagWindow);
            CreateButtons();

            pageControl.Source = BlobConverter.ConvertPDFBlobtoImage(0, pdfBinary,0);
            this.Content = documentGrid;
            this.AllowDrop = false;
            mainWindow.watchLibrary.list.Add(new FileList((BlobConverter.ConvertPDFBlobtoImage(0, pdfBinary, 0)), "PDF test", "tickle tickle", 2));
            DisplayPDF();
        }
        /// <summary>
        /// Sets the content of the class variable 'pdfBinary'.
        /// </summary>
        /// <param name="docId">The Document entity ID. Database parameter DocumentID.</param>
        private void setPdfByteArray(int docId)
        {
            try
            {
                var query = documents.Select(x => new { x.DocumentID, x.DocumentBinary }).Where(x => x.DocumentID == docId).FirstOrDefault();
                pdfBinary = query.DocumentBinary;
            }
            catch (Exception e)
            {
                MainWindow.warningWindow.Show(e.ToString());
            }
        }

        #region Events

        /// <summary>
        /// This event occurs when the DocumentContainer is finished loading. It centralizes the labels (lbl_productName, lbl_pageIndex).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void DocumentContainer_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(lbl_productName, -(lbl_productName.ActualWidth / 2));
            Canvas.SetLeft(lbl_pageIndex, -(lbl_pageIndex.ActualWidth / 2));
            naturalSize = this.RenderSize;
        }
        /// <summary>
        /// This event occurs when the nextbutton is clicked. It increases the value of the pageSlider if the index is within the bounds of the pageSlider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (pageIndex < pageSlider.Maximum)
                pageSlider.Value = ++pageIndex;
        }
        /// <summary>
        /// This event occurs when the previous button is clicked. It decreases the value of the pageSlider if the index is within the bounds of the pageSlider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_prev_Click(object sender, RoutedEventArgs e)
        {
            if (pageIndex > pageSlider.Minimum)
                pageSlider.Value = --pageIndex;
        }
        /// <summary>
        /// This event occurs when the close button is clicked. It close the documentContainer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ScatterWindow.Items.Remove(this);
            SurfaceListBoxItem tempItem = new SurfaceListBoxItem() { Content = documentName };
            foreach (SurfaceListBoxItem Item in tagWindow.lbx_Documents.Items)
                if (tempItem.Content.Equals(Item.Content))
                    Item.IsEnabled = true;
            //ClearSinglePages();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Container_ManipulationCompleted(object sender, RoutedEventArgs e)
        {
            ScatterViewItem _sender =  sender as ScatterViewItem;
            const int edgeTolerance = 25;
            this.AllowDrop = false;

            if (_sender.ActualCenter.X < edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 90, naturalSize);
            }
            else if (_sender.ActualCenter.X > System.Windows.SystemParameters.PrimaryScreenWidth - edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 270, naturalSize);
                
            }
            else if (_sender.ActualCenter.Y < edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 180, naturalSize);
                
            }
            else if (_sender.ActualCenter.Y > System.Windows.SystemParameters.PrimaryScreenHeight - edgeTolerance)
            {
                PublicMethods.organizeobjects(_sender, 0, naturalSize);
                
            }
        }

        
        /// <summary>
        /// This event occurs when the DocumentConainer is tapped. It duplicates the current page shown, if it is a double tap, as a new ScatterViewItem for the ScatterView (ScatterWindow)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void documentContainer_Clicked(object sender, TouchEventArgs e)
        {
            
            bool allowDoubleTap = MainWindow.configurationData.DoubleTap.Value;
          
            if (IsDoubleTap(e) && allowDoubleTap)
            {
                bool itemAlreadyExists = false;
                foreach (ScatterViewItem item in mainWindow.ScatterWindow.Items.OfType<ScatterViewItem>())
                {
                    try
                    {
                        System.Windows.Controls.Image ItemImage = item.Content as System.Windows.Controls.Image;
                        if (ItemImage.Source == pageControl.Source)
                            itemAlreadyExists = true;
                    }
                    catch (Exception ex)
                    {
                        //MainWindow.warningWindow.Show(ex.ToString());
                    }
                }
                if (!itemAlreadyExists)
                {
                    ScatterViewItem singlePage = new ScatterViewItem() { Content = new SysImages() { Source = pageControl.Source } };
                    tagWindow.listOfSingleImages.Add(singlePage);
                    singlePage.ContainerManipulationCompleted += (senderPage, args) =>
                    {
                        if ((singlePage.ActualCenter.X < this.ActualCenter.X + 50) && (singlePage.ActualCenter.X > this.ActualCenter.X - 50) && (singlePage.ActualCenter.Y < this.ActualCenter.Y + 50) && (singlePage.ActualCenter.Y > this.ActualCenter.Y - 50))
                            this.AllowDrop = true;
                    };
                    singlePage.Unloaded += (senderPage, args) =>
                    {
                        this.AllowDrop = false;
                        mainWindow.ScatterWindow.Items.Remove(singlePage);
                    };
                    singlePage.ContainerManipulationCompleted += Container_ManipulationCompleted;
                    //this.ContainerManipulationCompleted += DocumentContainer_ManipulationCompleted;
                    mainWindow.ScatterWindow.Items.Add(singlePage);
                    mainWindow.watchLibrary.list.Add(new FileList((BlobConverter.ConvertPDFBlobtoImage(pageIndex, pdfBinary,0)), "Single Page test", "tickle tickle", 2));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method is called by the constructor to initialize objects to be used.
        /// </summary>
        private void InitializeData()
        {
            documentGrid = new Grid();
            pageControl = new System.Windows.Controls.Image();
            listOfPages = new List<BitmapImage>();
            listOfSinglePages = new List<ScatterViewItem>();
            pageSlider = new SurfaceSlider();
            convertThread = new BackgroundWorker();

            documentGrid.Children.Add(pageControl);
        }
        /// <summary>
        /// This method is called once and draws the product name label (lbl_productName)
        /// </summary>
        private void DrawProductNameLabel()
        {
            Canvas nameCanvas = new Canvas();
            lbl_productName = new Label();
            lbl_productName.Content = documentName;
            lbl_productName.Foreground = new SolidColorBrush(Colors.White);
            lbl_productName.FontSize = (int)MainWindow.configurationData.FontSize;

            nameCanvas.VerticalAlignment = VerticalAlignment.Top;
            nameCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            nameCanvas.Margin = new Thickness(0, -25, 0, 0);

            nameCanvas.Children.Add(lbl_productName);
            documentGrid.Children.Add(nameCanvas);
        }
        /// <summary>
        /// This method creates a pageslider.
        /// </summary>
        private void CreatePageSlider()
        {
            pageSlider.Minimum = 0;
            PDFDoc doc = new PDFDoc(pdfBinary, pdfBinary.Length);
            pageSlider.Maximum = doc.GetPageCount() - 1;
            pageSlider.Orientation = System.Windows.Controls.Orientation.Vertical;
            pageSlider.IsDirectionReversed = true;
            pageSlider.HorizontalAlignment = HorizontalAlignment.Right;
            pageSlider.Margin = new Thickness(0, 0, -40, 0);
            pageSlider.ValueChanged += SliderValueChanged;

            documentGrid.Children.Add(pageSlider);
        }

        /// <summary>
        /// This method creates the coloured frame around the document container.
        /// </summary>
        /// <param name="tw"></param>
        private void CreateDocumentFrame(TagWindow tw)
        {
            colourFrame = new Border();
            colourFrame.BorderThickness = new Thickness((int)MainWindow.configurationData.BoxColorFrameThickness);
            colourFrame.BorderBrush = new SolidColorBrush(tw.GetTagColour());
            documentGrid.Children.Add(colourFrame);
        }

        /// <summary>
        /// This method is called once and draws the label (lbl_pageIndex), setting its startvalue to 1.
        /// </summary>
        private void DrawIndexLabel()
        {
            Canvas indexCanvas = new Canvas();
            lbl_pageIndex = new Label();
            lbl_pageIndex.Content = (pageIndex + 1).ToString();
            lbl_pageIndex.HorizontalAlignment = HorizontalAlignment.Center;
            lbl_pageIndex.Foreground = new SolidColorBrush(Colors.White);

            indexCanvas.VerticalAlignment = VerticalAlignment.Bottom;
            indexCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            indexCanvas.Margin = new Thickness(0, 0, 0, -4);

            indexCanvas.Children.Add(lbl_pageIndex);
            documentGrid.Children.Add(indexCanvas);
        }
        /// <summary>
        /// This method is called once to create all buttons in the DocumentContainer (btn_next, btn_prev, btn_close).
        /// </summary>
        private void CreateButtons()
        {
            StreamResourceInfo nextStreamInfo = Application.GetResourceStream(new Uri("Resources/btnTransNext.png", UriKind.Relative));
            BitmapFrame nextButtonImage = BitmapFrame.Create(nextStreamInfo.Stream);
            ImageBrush nextImgBrush = new ImageBrush();
            nextImgBrush.ImageSource = nextButtonImage;

            SurfaceButton btn_next = new SurfaceButton();
            btn_next.Background = nextImgBrush;
            btn_next.Click += btn_next_Click;

            btn_next.VerticalAlignment = VerticalAlignment.Bottom;
            btn_next.HorizontalAlignment = HorizontalAlignment.Right;
            btn_next.MinWidth = 30;
            btn_next.MinHeight = 20;
            btn_next.Margin = new Thickness(0, 0, 0, -30);

            StreamResourceInfo prevStreamInfo = Application.GetResourceStream(new Uri("Resources/btnTransPrevious.png", UriKind.Relative));
            BitmapFrame prevButtonImage = BitmapFrame.Create(prevStreamInfo.Stream);
            ImageBrush prevImgBrush = new ImageBrush();
            prevImgBrush.ImageSource = prevButtonImage;

            SurfaceButton btn_prev = new SurfaceButton();
            btn_prev.Background = prevImgBrush;
            btn_prev.Click += btn_prev_Click;

            btn_prev.VerticalAlignment = VerticalAlignment.Bottom;
            btn_prev.HorizontalAlignment = HorizontalAlignment.Left;
            btn_prev.MinWidth = 30;
            btn_prev.MinHeight = 20;
            btn_prev.Margin = new Thickness(0, 0, 0, -30);

            StreamResourceInfo closeStreamInfo = Application.GetResourceStream(new Uri("Resources/btnX.png", UriKind.Relative));
            BitmapFrame closeButtonImage = BitmapFrame.Create(closeStreamInfo.Stream);
            ImageBrush closeImgBrush = new ImageBrush();
            closeImgBrush.ImageSource = closeButtonImage;

            SurfaceButton btn_close = new SurfaceButton();
            btn_close.Background = closeImgBrush;
            btn_close.Click += btn_close_Click;

            btn_close.VerticalAlignment = VerticalAlignment.Top;
            btn_close.HorizontalAlignment = HorizontalAlignment.Left;
            btn_close.MinWidth = (int)MainWindow.configurationData.CloseButtonSize;
            btn_close.MinHeight = (int)MainWindow.configurationData.CloseButtonSize;
            btn_close.Margin = new Thickness(-28, 3, 0, 0);

            documentGrid.Children.Add(btn_next);
            documentGrid.Children.Add(btn_prev);
            documentGrid.Children.Add(btn_close);
        }

        /// <summary>
        /// This method adds this DocumentContainer to the ScatterView (ScatterWindow) in the MainWindow.
        /// </summary>
        private void DisplayPDF()
        {
            mainWindow.ScatterWindow.Items.Add(this);
            tagWindow.pgb_Document.Visibility = Visibility.Hidden;
            tagWindow.lbx_Documents.IsEnabled = true;
        }

        /// <summary>
        /// This method is called to extract the name of the product from a given path.
        /// </summary>
        /// <returns></returns>
        private string ExtractNameFromPath(string path)
        {
            string[] substrings = path.Split('\\');
            return substrings[substrings.Length - 1];
        }

        /// <summary>
        /// This method is called whenever the slidervalue changes.
        /// </summary>
        private void SliderValueChanged(object sender, RoutedEventArgs e)
        {
            ChangePageToIndex(System.Convert.ToInt32(pageSlider.Value));
            pageIndex = System.Convert.ToInt32(pageSlider.Value);
        }

        /// <summary>
        /// This method is called when the slider value is changed, displaying a page at a specified index.
        /// </summary>
        private void ChangePageToIndex(int index)
        {
            lbl_pageIndex.Content = index + 1;
            lbl_pageIndex.HorizontalAlignment = HorizontalAlignment.Center;
            pageControl.Source = BlobConverter.ConvertPDFBlobtoImage(index, pdfBinary,0);
        }

        /// <summary>
        /// This method is called when a page is tapped to determine whether or not it is a double tap.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsDoubleTap(TouchEventArgs e)
        {
            System.Windows.Point currentTapPosition = e.GetTouchPoint(this).Position;
            bool tapsAreWithinPage = true;
            foreach (SurfaceButton button in documentGrid.Children.OfType<SurfaceButton>()) // Checks if the touch point targets a button within the documentGrid,
                if (e.TouchDevice.Target == button) { tapsAreWithinPage = false; }          // if so it is certain the taps are not within the bounds of the page itself
            bool tapsAreCloseInDistance = (                                                 // Detremines whether the taps are close in distance or not
                (currentTapPosition.X >= lastTapLocation.X - 15 &&
                currentTapPosition.X <= lastTapLocation.X + 15) &&
                (currentTapPosition.Y >= lastTapLocation.Y - 15 &&
                currentTapPosition.Y <= lastTapLocation.Y + 15));
            lastTapLocation = currentTapPosition;

            TimeSpan elapsed = doubleTapStopwatch.Elapsed;
            doubleTapStopwatch.Restart();
            bool tapsAreCloseInTime = (elapsed != TimeSpan.Zero && elapsed < TimeSpan.FromSeconds(0.7));

            return tapsAreCloseInDistance && tapsAreCloseInTime && tapsAreWithinPage;       // Returns a boolean of whether the last touch qualifies as part of a double tap or not (true or false)
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /*public void ClearSinglePages()
        {
            foreach (ScatterViewItem singlePage in listOfSinglePages)
                mainWindow.ScatterWindow.Items.Remove(singlePage);
        }*/
    }
}
