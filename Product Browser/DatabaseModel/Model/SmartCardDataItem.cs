using System;
using System.Collections.Generic;
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
    /// </summary>
    public class SmartCardDataItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public SmartCardDataItemCategory Category { get; private set; }

        public byte[] Data { get; private set; }

        /// <summary>
        /// Retrieves an image from the Data byte array. Returns null if Data is null
        /// or if Category does not match "image"
        /// </summary>
        /// <returns></returns>
        public Bitmap GetImage()
        {
            if (Category != SmartCardDataItemCategory.Image || Data == null && Data.Length == 0)
                return null;

            Bitmap image = null;
                
            try
            {
                image = (Bitmap)Bitmap.FromStream(new MemoryStream(Data));
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error loading image file...");
            }

            return image;
        }

        /// <summary>
        /// Retrieves the document from the Data array as an image source which can be used directly in
        /// an Image WPF element.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public ImageSource GetImageSource()
        {
            Bitmap image = GetImage();

            if (image == null)
                return null;

            ImageSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                      image.GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());

            return source;
        }

        /// <summary>
        /// Retrives a video from Data and stores it to disk, then returns with a Uri to that file,
        /// for use with the MediaElement.source property. Returns null if Data is null or if Category does not match "image".
        /// </summary>
        /// <returns></returns>
        public Uri GetVideo()
        {
            if (Category != SmartCardDataItemCategory.Video || Data == null || Data.Length == 0)
                return null;

            string path = @"TempVideo\" + Name;

            try
            {
                System.IO.File.WriteAllBytes(path, Data);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error attempting to temporarily save video file to disk...");
                return null;
            }

            return new Uri(path, UriKind.Relative);
        }

        /// <summary>
        /// Retrieves the document from the Data array as a list of images.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public List<Bitmap> GetDocumentAsBitmapList()
        {
            if (Category != SmartCardDataItemCategory.Document || Data == null || Data.Length == 0)
                return null;

            List<Bitmap> images = new List<Bitmap>();

            int current = 0;
            while (current < Data.Length)
            {
                Bitmap newImage = null;

                // Find size of next element
                int elementSize = BitConverter.ToInt32(Data, current);
                current += 4; // Skip 4 forward, since we read those already with ToInt32

                byte[] imageBytes = new byte[elementSize];
                for (int i = 0; i < elementSize; i++)
                    imageBytes[i] = Data[current++]; // Transfer

                if (current == Data.Length) // This was the last element, which is the original pdf document. Ignore and break
                    break;

                // Else we have an image, convert the byte array
                MemoryStream imageStream = new MemoryStream(imageBytes); // No point disposing of memory stream, it doesn't lock any resources

                newImage = (Bitmap)Bitmap.FromStream(imageStream);

                images.Add(newImage);
            }

            return images;
        }

        /// <summary>
        /// Retrieves the document from the Data array as a list of image sources which can be used directly in
        /// an Image WPF element.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public List<ImageSource> GetDocumentAsImageSources()
        {
            List<Bitmap> images = GetDocumentAsBitmapList();

            if (images == null || images.Count == 0)
                return null;

            List<ImageSource> sources = new List<ImageSource>(images.Count);

            for (int i = 0; i < images.Count; i++)
            {
                ImageSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                      images[i].GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());
                sources.Add(source);
            }

            return sources;
        }

        /// <summary>
        /// Retrieves the original PDF from the Data array.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public byte[] GetOriginalDocument()
        {
            if (Category != SmartCardDataItemCategory.Document || Data == null || Data.Length == 0)
                return null;

            byte[] document = null;

            int current = 0, startOfPrevious = 0;
            while (current < Data.Length)
            {
                // Find size of next element
                int elementSize = BitConverter.ToInt32(Data, current);
                current += 4 + elementSize; // Skip 4 forward, since we read those already with ToInt32
                startOfPrevious = current - elementSize;
            }

            int documentLength = current - startOfPrevious;
            document = new byte[documentLength];

            for (int i = 0; i < documentLength; i++)
                document[i] = Data[startOfPrevious++];

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

            ImageConverter converter = new ImageConverter();

            long numbPages = pdf.GetPageCount();

            byte[][] tempImageArr = new byte[numbPages + 1][];

            int totalSize = 0;
            for (int i = 0; i < numbPages; i++)
            {
                // Store each page in the array as BMP
                tempImageArr[i] = (byte[])converter.ConvertTo(draw.GetBitmap(pdf.GetPage(i+1)), typeof(byte[]));
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
        /// Constructor. Use this rather than setting Data directly, since Documents needs to be converted
        /// </summary>
        /// <param name="name">The data item's name</param>
        /// <param name="category">The data item's category; document expects pdf!</param>
        /// <param name="data">The data item's raw data</param>
        public SmartCardDataItem(string name, SmartCardDataItemCategory category, byte[] data)
        {
            Name = name;
            Category = category;
            
            // Copy byte array, since it may come from HttpStream?
            Data = new byte[data.Length];
            for (int i = 0; i < Data.Length; i++)
                Data[i] = data[i];

            if (category == SmartCardDataItemCategory.Document) // If we have a document, split it up into images
                Data = ConvertPDFToImageArray(Data);
        }

        /// <summary>
        /// Private constructor for Entity Framework?
        /// </summary>
        private SmartCardDataItem() { }
    }
}
