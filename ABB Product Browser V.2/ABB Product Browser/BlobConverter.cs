using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data;
using System.Data.Linq;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using pdftron.PDF;
using System.ServiceProcess;

namespace ProductBrowser
{
    /// <summary>
    /// Provides static methods for converting BLOBs.
    /// </summary>
    public class BlobConverter
    {
        /// <summary>
        /// LINQ datacontext object.
        /// </summary>
        private static ABBDataClassesDataContext dc;
        /// <summary>
        /// Table of Document entities.
        /// </summary>
        private static Table<Document> docs;

        private static int restartMssqlServiceAttempt;

        /// <summary>
        /// Constructor.
        /// </summary>
        static BlobConverter()
        {
            dc = new ABBDataClassesDataContext();
            docs = dc.GetTable<Document>();
            restartMssqlServiceAttempt = 0;
        }
        /// <summary>
        /// Builds an ObservableCollection of FileList objects using Document entities specified by an offering name. 
        /// </summary>
        /// <param name="offeringName">The offering name bound to the documents. Database property OfferingName.</param>
        /// <param name="tagWindow">The TagWindow property of the FileList object.</param>
        /// <param name="mainWindow">The MainWindow property of the FileList object.</param>
        /// <returns>The ObservableCollection of FileList objects.</returns>
        public static ObservableCollection<FileList> addDocumentsByOfferingName(string offeringName, TagWindow tagWindow, MainWindow mainWindow)
        {
            ObservableCollection<FileList> fileList = new ObservableCollection<FileList>();
            var documents = docs.Select(data => new { data.DocumentBinary, data.CategoryName, data.OfferingName, data.Title, data.DocumentID }).Where(data => data.OfferingName == offeringName);

            try
            {
                foreach (var item in documents)
                {
                    fileList.Add(new FileList(BlobConverter.ConvertPDFBlobtoImage(0, item.DocumentBinary,200), item.CategoryName, item.Title, item.DocumentID, tagWindow, mainWindow));
                }
            }
            catch (Exception e)
            {
                DBHelper.RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Documents);
                    addDocumentsByOfferingName(offeringName, tagWindow, mainWindow);
                }
                catch (Exception ex)
                {
                    /*
                    using (var sc = new ServiceController("MSSQLSERVER"))
                        MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                    */
                }
            }
            return fileList;
        }
        /// <summary>
        /// Converts an image BLOB to a BitmapSource.
        /// </summary>
        /// <param name="imgBinary">The image BLOB to convert.</param>
        /// <returns>The BitmapSource representing the image BLOB.</returns>
        public static BitmapSource ConvertImgBlobtoImage(byte[] imgBinary)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(imgBinary))
            {

                mem.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = null;
                bitmapImage.StreamSource = mem;
                bitmapImage.EndInit();
            }
            bitmapImage.Freeze();
            return (BitmapSource) bitmapImage;
        }

        /// <summary>
        /// Converts a PDF BLOB to a BitmapSource.
        /// </summary>
        /// <param name="index">The zero-based integer index of the PDF page.</param>
        /// <param name="pdfBinary">The PDF BLOB to convert.</param>
        /// <returns>The BitmapSource representing the PDF page.</returns>
        public static BitmapSource ConvertPDFBlobtoImage(int index, byte[] pdfBinary, int pWidth)
        {
            // Ubrukelig LibPdf kode:
            /*
            BitmapImage bitmapImage = new BitmapImage();
            using (var pdf = new LibPdf(pdfBinary)){
                byte[] pngBytes = pdf.GetImage(index + 1, ImageType.PNG);
                System.Drawing.Bitmap bitmap;
                //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(;
                using (MemoryStream memory = new MemoryStream(pngBytes))
                {
                    bitmap = new System.Drawing.Bitmap(memory);
                    memory.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
            }
            return bitmapImage;
            */

            pdftron.PDFNet.Initialize();
            BitmapImage bitmapImage = new BitmapImage();
            using (PDFDoc doc = new PDFDoc(pdfBinary, pdfBinary.Length))
            {
                doc.InitSecurityHandler();
                PDFDraw draw = new PDFDraw();
                System.Drawing.Bitmap bitmap = draw.GetBitmap(doc.GetPage(index + 1));
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Jpeg);
                    memory.Position = 0;
                    bitmapImage.BeginInit();
                    if (bitmap.Width > 850)
                        bitmapImage.DecodePixelWidth = 850;
                    if (pWidth != 0)
                        bitmapImage.DecodePixelWidth = pWidth;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
            }
            return (BitmapSource) bitmapImage;
        }
    }
}
