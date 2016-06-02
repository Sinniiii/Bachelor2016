using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using pdftron;
using pdftron.PDF;
using Image = System.Drawing.Image;

namespace DatabaseModel.Model
{
    public enum SmartCardDataItemCategory
    {
        Image,
        Document,
        Video
    }

    /// <summary>
    /// A data item kept by Product. Could be an image, document(pdf) or video.
    /// 
    /// If the data is a video, it will be loaded from HDD instead of database, due to performance and memory constraints.
    /// These files should be saved to VIDEO_FOLDER + SmartCard.TagId + videoname 
    /// </summary>
    public class SmartCardDataItem
    {
        [NotMapped]
        public const string VIDEO_FOLDER = @"c:\ABBProductBrowser\Videos\Tag\";

        public int Id { get; private set; }
        
        public string Name { get; private set; }

        public SmartCardDataItemCategory Category { get; private set; }
        
        public virtual SmartCardDataItemData DataField { get; private set; }
        
        public SmartCard SmartCard { get; private set; }

        /// <summary>
        /// Retrieves the image from the Data array as an image source which can be used directly in
        /// an Image WPF element.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetImageSource()
        {
            if (Category != SmartCardDataItemCategory.Image || DataField == null || DataField.Data.Length == 0)
                return null;

            BitmapImage newImage = new BitmapImage();

            var mem = new MemoryStream(DataField.Data);
            mem.Position = 0;
            newImage.BeginInit();
            newImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            newImage.CacheOption = BitmapCacheOption.OnLoad;
            newImage.UriSource = null;
            newImage.StreamSource = mem;
            newImage.EndInit();

            mem.Close(); mem.Dispose();

            newImage.Freeze();
            
            return newImage;
        }

        public BitmapImage GetTumbnailImageSource()
        {
            if (Category != SmartCardDataItemCategory.Image || DataField == null || DataField.Data.Length == 0)
                return null;

            BitmapImage newImage = new BitmapImage();

            var mem = new MemoryStream(DataField.Data);
            mem.Position = 0;
            newImage.BeginInit();
            newImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            newImage.CacheOption = BitmapCacheOption.OnLoad;
            newImage.UriSource = null;
            newImage.StreamSource = mem;

            newImage.DecodePixelWidth = 128;

            newImage.EndInit();

            mem.Close(); mem.Dispose();

            newImage.Freeze();

            return newImage;
        }

        /// <summary>
        /// Retrives a video from Data and stores it to disk, then returns with a Uri to that file,
        /// for use with the MediaElement.source property. Returns null if Data is null or if Category does not match "image".
        /// </summary>
        /// <returns></returns>
        public Uri GetVideo()
        {
            if (Category != SmartCardDataItemCategory.Video)
                return null;

            string path = VIDEO_FOLDER + SmartCard.TagId + @"\" + Name;

            if (File.Exists(path))
                return new Uri(path, UriKind.Absolute);
            else
                return null;
        }

        /// <summary>
        /// Retrieves the document from the Data array as a list of images.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public List<BitmapImage> GetDocumentAsImageSources()
        {
            if (Category != SmartCardDataItemCategory.Document || DataField == null || DataField.Data.Length == 0)
                return null;

            List<BitmapImage> images = new List<BitmapImage>();
            
            int current = 0;
            while (current < DataField.Data.Length)
            {
                BitmapImage newImage = new BitmapImage();
                
                // Find size of next element
                int elementSize = BitConverter.ToInt32(DataField.Data, current);
                current += 4; // Skip 4 forward, since we read those already with ToInt32

                byte[] imageBytes = new byte[elementSize];
                for (int i = 0; i < elementSize; i++)
                    imageBytes[i] = DataField.Data[current++]; // Transfer

                if (current == DataField.Data.Length) // This was the last element, which is the original pdf document. Ignore and break
                    break;
                
                var mem = new MemoryStream(imageBytes);
                mem.Position = 0;
                newImage.BeginInit();
                newImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat & BitmapCreateOptions.DelayCreation;
                newImage.CacheOption = BitmapCacheOption.None;
                newImage.UriSource = null;
                newImage.StreamSource = mem;
                newImage.EndInit();

                newImage.Freeze();

                images.Add(newImage);
            }
            return images;
        }

        /// <summary>
        /// Retrieves a page from the document as an image.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetPageFromDocumentAsImageSource(int page)
        {
            if (Category != SmartCardDataItemCategory.Document || DataField == null || DataField.Data.Length == 0)
                return null;

            BitmapImage image = new BitmapImage();

            int index = 0;
            int current = 0;
            while (current < DataField.Data.Length)
            {
                // Find size of next element
                int elementSize = BitConverter.ToInt32(DataField.Data, current);
                current += 4; // Skip 4 forward, since we read those already with ToInt32

                if(index != page)
                {
                    current += elementSize;
                    index++;
                    continue;
                }

                byte[] imageBytes = new byte[elementSize];
                for (int i = 0; i < elementSize; i++)
                    imageBytes[i] = DataField.Data[current++]; // Transfer

                if (current == DataField.Data.Length) // This was the last element, which is the original pdf document. Ignore and break
                    break;

                var mem = new MemoryStream(imageBytes);
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();

                mem.Close();mem.Dispose();

                image.Freeze();

                break;
            }

            return image;
        }

        /// <summary>
        /// Retrieves the document from the Data array as a list of thumbnail images.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public List<BitmapImage> GetDocumentAsThumbnailImageSources()
        {
            if (Category != SmartCardDataItemCategory.Document || DataField == null || DataField.Data.Length == 0)
                return null;

            List<BitmapImage> images = new List<BitmapImage>();

            int current = 0;
            while (current < DataField.Data.Length)
            {
                BitmapImage newImage = new BitmapImage();

                // Find size of next element
                int elementSize = BitConverter.ToInt32(DataField.Data, current);
                current += 4; // Skip 4 forward, since we read those already with ToInt32

                byte[] imageBytes = new byte[elementSize];
                for (int i = 0; i < elementSize; i++)
                    imageBytes[i] = DataField.Data[current++]; // Transfer

                if (current == DataField.Data.Length) // This was the last element, which is the original pdf document. Ignore and break
                    break;

                var mem = new MemoryStream(imageBytes);
                mem.Position = 0;
                newImage.BeginInit();
                newImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                newImage.CacheOption = BitmapCacheOption.OnLoad;
                newImage.UriSource = null;
                newImage.StreamSource = mem;

                newImage.DecodePixelWidth = 64;
                newImage.EndInit();

                mem.Close();mem.Dispose();

                newImage.Freeze();

                images.Add(newImage);
            }

            return images;
        }

        /// <summary>
        /// Retrieves the original PDF from the Data array.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public byte[] GetOriginalDocument()
        {
            if (Category != SmartCardDataItemCategory.Document || DataField == null || DataField.Data.Length == 0)
                return null;

            byte[] document = null;

            int current = 0, startOfPrevious = 0;
            while (current < DataField.Data.Length)
            {
                // Find size of next element
                int elementSize = BitConverter.ToInt32(DataField.Data, current);
                current += 4 + elementSize; // Skip 4 forward, since we read those already with ToInt32
                startOfPrevious = current - elementSize;
            }

            int documentLength = current - startOfPrevious;
            document = new byte[documentLength];

            for (int i = 0; i < documentLength; i++)
                document[i] = DataField.Data[startOfPrevious++];

            return document;
        }

        /// <summary>
        /// Converts a byte[] containing a pdf document into a byte[] containing that same pdf converted into an array
        /// of images and the original document, separated by a 4-byte(from int) page-image length descriptor
        /// </summary>
        /// <param name="pdfData"></param>
        /// <returns></returns>
        private byte[] ConvertPDFToImageArray(byte[] pdfData)
        {
            if (pdfData.Length == 0)
                return null;

            PDFNet.Initialize();
            PDFDoc pdf = new PDFDoc(pdfData, pdfData.Length);
            pdf.InitSecurityHandler();
            PDFDraw draw = new PDFDraw();
            draw.SetDPI(150); // HMM

            ImageConverter converter = new ImageConverter();

            long numbPages = pdf.GetPageCount();

            byte[][] tempImageArr = new byte[numbPages + 1][];

            int totalSize = 0;
            for (int i = 0; i < numbPages; i++)
            {
                // Store each page in the array as BMP
                tempImageArr[i] = (byte[])converter.ConvertTo(draw.GetBitmap(pdf.GetPage(i + 1)), typeof(byte[]));
                totalSize += tempImageArr[i].Length;
            }

            tempImageArr[numbPages] = pdfData; // Also store the original PDF
            totalSize += tempImageArr[numbPages].Length;

            // Convert to single array
            byte[] newData = new byte[totalSize + (numbPages + 1) * 4]; // Reserve 4 bytes at the start of every entry for entry length

            int tempImageArrLength = tempImageArr.Length;
            int current = 0;
            for (int i = 0; i < tempImageArrLength; i++)
            {

                byte[] fromInt = BitConverter.GetBytes(tempImageArr[i].Length); // Returns a 4 byte array containing length of page-image
                for (int u = 0; u < 4; u++) // Place that array at the beginning of each entry
                    newData[current++] = fromInt[u];

                int lengthArray = tempImageArr[i].Length;
                for (int u = 0; u < lengthArray; u++)
                    newData[current++] = tempImageArr[i][u];
            }

            return newData;
        }

        /// <summary>
        /// Constructor. Use this rather than setting Data directly, since Documents needs to be converted. Not for video files!
        /// </summary>
        /// <param name="name">The data item's name</param>
        /// <param name="category">The data item's category; document expects pdf!</param>
        /// <param name="data">The data item's raw data</param>
        public SmartCardDataItem(string name, SmartCardDataItemCategory category, byte[] data)
        {
            Name = name;
            Category = category;

            if (category == SmartCardDataItemCategory.Video)
                return;
            else if (category == SmartCardDataItemCategory.Document) // If we have a document, split it up into images
                DataField = new SmartCardDataItemData(ConvertPDFToImageArray(data));
            else // Image
                DataField = new SmartCardDataItemData(data);
        }

        /// <summary>
        /// Constructor for video data items that are stored on HDD
        /// </summary>
        /// <param name="name"></param>
        public SmartCardDataItem(string name)
        {
            Name = name;
            Category = SmartCardDataItemCategory.Video;
        }

        // For Entity framework
        private SmartCardDataItem() { }
    }
}
