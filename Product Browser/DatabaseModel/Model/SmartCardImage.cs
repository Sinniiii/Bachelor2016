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
    /// <summary>
    /// A image contained by smartcard
    /// </summary>
    public class SmartCardImage
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public virtual SmartCardImageData DataField { get; private set; }

        public SmartCard SmartCard { get; private set; }

        /// <summary>
        /// Retrieves the image from the Data array as an image source which can be used directly in
        /// an Image WPF element.
        /// Returns null if Data is null or if Category does not match "document".
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetImageSource()
        {
            if (DataField == null || DataField.Data.Length == 0)
                return null;

            BitmapImage newImage = new BitmapImage();

            var mem = new MemoryStream(DataField.Data);
            mem.Position = 0;
            newImage.BeginInit();
            newImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            newImage.CacheOption = BitmapCacheOption.None;
            newImage.UriSource = null;
            newImage.StreamSource = mem;
            newImage.EndInit();

            newImage.Freeze();

            return newImage;
        }

        /// <summary>
        /// Constructor. Use this rather than setting Data directly, since Documents needs to be converted. Not for video files!
        /// </summary>
        /// <param name="name">The data item's name</param>
        /// <param name="category">The data item's category; document expects pdf!</param>
        /// <param name="data">The data item's raw data</param>
        public SmartCardImage(string name, byte[] data)
        {
            Name = name;

            DataField = new SmartCardImageData(data);
        }

        // For Entity framework
        private SmartCardImage() { }
    }
}
