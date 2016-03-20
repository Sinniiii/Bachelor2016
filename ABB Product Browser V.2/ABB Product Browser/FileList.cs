using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data;
using System.Data.Linq;
using System.Collections.ObjectModel;
using SysImages = System.Windows.Controls.Image;
using System.Windows.Controls;

namespace ProductBrowser
{
    public class FileList
    {
        private BitmapSource bitmap;

        public FileList(BitmapSource bitmapImage, string categoryType, string title, int documentId, TagWindow tagWindow, MainWindow mainWindow)
        {
            this.Bitmap = bitmapImage;
            CategoryType = categoryType;
            Title = title;
            Main = mainWindow;
            Tag = tagWindow;
            DocumentId = documentId;
        }

        public FileList(string fileType, string offeringName, string title, string filePath, string categoryType)
        {
            FileType = fileType;
            OfferingName = offeringName;
            Title = title;
            FilePath = filePath;
            CategoryType = categoryType;
        }

        public FileList(int offeringID, string offeringName, string parent, int parentId)
        {
            OfferingID = offeringID;
            OfferingName = offeringName;
            Parent = parent;
            ParentID = parentId;
        }

        public FileList(BitmapSource bitmapImage, string fileType, string title, int Id)
        {
            this.Bitmap = bitmapImage;
            CategoryType = fileType;
            Title = title;
            ID = Id;
        }

        public FileList(Grid grid, double radius, int index, bool show)
        {
            ButtonGrid = grid;
            Radius = radius;
            Index = index;
            ShowButton = show;
        }

        public FileList(BitmapSource bitmapImage, string filePath)
        {
            this.Bitmap = bitmapImage;
            FilePath = filePath;
        }

        public FileList(string usbName, string usbRoot, int index)
        {
            UsbName = usbName;
            UsbRoot = usbRoot;
            Index = index;
        }

        public BitmapSource Bitmap
        {
            get
            {
                return bitmap;
            }
            set
            {
                bitmap = value;
            }
        }

        public string FileType { get; set; }

        public string OfferingName { get; set; }

        public string FilePath { get; set; }
        
        public string CategoryType { get; set; }

        public string Title { get; set; }

        public int DocumentId { get; set; }

        public int ID { get; set; }

        public MainWindow Main { get; set; }

        public TagWindow Tag { get; set; }

        public string UsbName { get; set; }

        public string UsbRoot { get; set; }

        public int OfferingID { get; set; }
        
        public string Parent { get; set; }
        
        public int ParentID { get; set; }

        public Grid ButtonGrid { get; set; }

        public double Radius { get; set; }

        public int Index { get; set; }

        public bool ShowButton { get; set; }



    }
}
