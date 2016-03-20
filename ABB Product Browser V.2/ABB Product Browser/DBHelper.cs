using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using pdftron.PDF;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceProcess;
using System.Diagnostics;

namespace ProductBrowser
{
    /// <summary>
    /// Provides static methods for database handling.
    /// </summary>
    public class DBHelper
    {
        /// <summary>
        /// LINQ datacontext object.
        /// </summary>
        private static ABBDataClassesDataContext dc;

        private static ObservableCollection<FileList> clone;

        private static int restartMssqlServiceAttempt;

        private static int insertAttempts;

        private static int waitDuration;
        /// <summary>
        /// Constructor.
        /// </summary>
        static DBHelper()
        {
            dc = new ABBDataClassesDataContext();
            restartMssqlServiceAttempt = 0;
            insertAttempts = 0;
            waitDuration = 7;
            clone = new ObservableCollection<FileList>();
        }

        public static List<StringData> ReadConfigSettings()
        {
            List<StringData> list = new List<StringData>();
            try
            {
                var query =
                    (from s in dc.Settings
                    where s.SettingName.Equals("Default")
                    select s).First();

                StringData sd = new StringData();
                sd.stringTag = "RootFolder";
                sd.stringValue = query.RootFolder;
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "Language";
                sd.stringValue = query.Language;
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "FontSize";
                sd.stringValue = query.FontSize.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "LostTagTimeout";
                sd.stringValue = query.LostTagTimeout.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "OrientationOffsetFromTag";
                sd.stringValue = query.OrientationOffsetFromTag.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "BoxColorFrameThickness";
                sd.stringValue = query.BoxColorFrameThickness.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "CardColorFrameThickness";
                sd.stringValue = query.CardColorFrameThickness.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "CloseButtonSize";
                sd.stringValue = query.CloseButtonSize.ToString();
                list.Add(sd);
                sd = new StringData();
                sd.stringTag = "DoubleTap";
                sd.stringValue = query.DoubleTap.ToString();
                list.Add(sd);
            }
            catch { }
            return list;
        }



        /// <summary>
        /// Acquires the video title with specified offering ID and index. 
        /// </summary>
        /// <param name="offeringID">The offering ID bound to this video. Database property OfferingID.</param>
        /// <param name="index">The zero-based index in an ordered list of Video entities.</param>
        /// <returns>The video title. Database property Title.</returns>
        public static string GetVideoTitle(int offeringID, int index)
        {
            string title = null;
            var query =
                from v in dc.Videos
                where v.OfferingID == offeringID
                orderby v.VideoID
                select v.Title;

            int x = 0;
            try
            {
                foreach (string v in query)
                {
                    if (x == index)
                    {
                        title = v;
                        break;
                    }
                    else
                    {
                        x++;
                    }
                }
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                    return GetVideoTitle(offeringID, index);
                }
                catch (Exception ex)
                {
                    using (var sc = new ServiceController("MSSQLSERVER"))
                        MainWindow.warningWindow.Show(sc.ServiceName + " couldn't load all videos. Please close and reload the videocontainer.");
                }
            }
            return title;
        }

        public static byte[] GetVideo(int offeringID, string title)
        {
            byte[] video = null;
            
            try
            {
                var query =
                (from v in dc.Videos
                 where v.Title == title && v.OfferingID == offeringID
                 orderby v.VideoID
                 select v.VideoBinary).FirstOrDefault();
                video = query;
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                    return GetVideo(offeringID, title);
                }
                catch (Exception ex)
                {
                    /*
                      using (var sc = new ServiceController("MSSQLSERVER"))
                          MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                      */
                }
            }
            return video;
        }

        /// <summary>
        /// Acquires the video BLOB with specified offering ID and index.
        /// </summary>
        /// <param name="offeringID">The offering ID bound to this video. Database property OfferingID.</param>
        /// <param name="index">The zero-based index in an ordered list of Video entities.</param>
        /// <returns>The video BLOB. Database property VideoBinary.</returns>
        /*
        public static byte[] GetVideo(int offeringID, int index)
        {
            byte[] video = null;
            var query =
                from v in dc.Videos
                where v.OfferingID == offeringID
                orderby v.VideoID
                select v.VideoBinary;

            int x = 0;
            try
            {
                foreach (byte[] v in query)
                {
                    if (x == index)
                    {
                        video = v;
                        break;
                    }
                    else
                    {
                        x++;
                    }
                }
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                    return GetVideo(offeringID, index);
                }
                catch (Exception ex)
                {
                    
                }
            }
            return video;
        }
        */
        /// <summary>
        /// Acquires the bytetag entity with specified integer value. 
        /// </summary>
        /// <param name="tagID">The bytetag integer ID. This is equal to its hexadecimal value + 1. Database property TagID.</param>
        /// <returns>The Tag entity.</returns>
        public static Tag GetTag(int tagID)
        {
            Tag tag = new Tag();

            var query1 =
                from t in dc.Tags
                where t.TagID == tagID
                select t;

            foreach (Tag t in query1)
            {
                tag = t;
            }
            return tag;
        }
        /// <summary>
        /// Acquires the offering entity with specified offering name. 
        /// </summary>
        /// <param name="offeringName">The offering name bound to this offering. Database property OfferingName.</param>
        /// <returns>The Offering entity.</returns>
        public static Offering GetOffering(string offeringName)
        {
            Offering offering = new Offering();

            var query =
                from o in dc.Offerings
                where o.OfferingName == offeringName
                select o;

            foreach (Offering o in query)
            {
                offering = o;
            }
            return offering;
        }
        /// <summary>
        /// Acquires a List of Image entities with specified offering name.
        /// </summary>
        /// <param name="offeringName">The offering name bound to the images. Database property OfferingName.</param>
        /// <returns>The List of Image entities.</returns>
        public static List<Image> GetImages(string offeringName)
        {
            List<Image> images = new List<Image>();

            var query =
                from img in dc.Images
                where img.OfferingName == offeringName
                select img;

            foreach (Image img in query)
            {
                images.Add(img);
            }
            return images;
        }
        /// <summary>
        /// Acquires a List of Document entities with specified offering name.
        /// </summary>
        /// <param name="offeringName">The offering name bound to the documents. Database property OfferingName.</param>
        /// <returns>The List of Document entities.</returns>
        public static List<Document> GetDocuments(string offeringName)
        {
            List<Document> documents = new List<Document>();

            var query =
                from doc in dc.Documents
                where doc.OfferingName == offeringName
                select doc;

            foreach (Document doc in query)
            {
                documents.Add(doc);
            }
            return documents;
        }
        /// <summary>
        /// Acquires a List of Video entities with specified offering name.
        /// </summary>
        /// <param name="offeringName">The offering name bound to the videos. Database property OfferingName.</param>
        /// <returns>The List of Video entities.</returns>
        public static List<Video> GetVideos(string offeringName)
        {
            List<Video> videos = new List<Video>();

            var query =
                from vid in dc.Videos
                where vid.OfferingName == offeringName
                select vid;

            foreach (Video vid in query)
            {
                videos.Add(vid);
            }
            return videos;
        }
        // kan kanskje forbedres
        public static int numberOfVideos(string offeringName)
        {
            int number = 0;

            var query =
                from v in dc.Videos
                where v.OfferingName == offeringName
                select v.Title;
            try
            {
                number = query.Count();

            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                    return numberOfVideos(offeringName);
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
            return number;
        }


        /// <summary>
        /// Converts a pdf BLOB to a List of BitmapImage objects.
        /// </summary>
        /// <param name="pdfBinary">The pdf BLOB to convert.</param>
        /// <returns>The List of BitmapImage objects</returns>
        public static List<BitmapImage> ConvertPDFBlobToImageList(byte[] pdfBinary)
        {
            List<BitmapImage> result = new List<BitmapImage>();
            pdftron.PDFNet.Initialize();
            using (PDFDoc doc = new PDFDoc(pdfBinary, pdfBinary.Length))
            {
                doc.InitSecurityHandler();
                PDFDraw draw = new PDFDraw();
                for (int i = 1; i <= doc.GetPageCount(); i++)
                {
                    System.Drawing.Bitmap bitmap = draw.GetBitmap(doc.GetPage(i));
                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bitmap.Save(memory, ImageFormat.Jpeg);
                        memory.Position = 0;
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                    }
                    result.Add(bitmapImage);
                }
            }
            return result;
        }
        /// <summary>
        /// Attempts to insert a Offering entity. 
        /// </summary>
        /// <param name="offering">The Offering entity to insert.</param>
        public static void InsertOffering(Offering offering)
        {
            var query1 =
                (from o in dc.Offerings
                 select o).Distinct();

            bool exists = false;
            try
            {
                foreach (Offering o in query1)
                {
                    if ((o.Equals(offering.OfferingName)) && (o.ParentID != offering.ParentID))
                    {
                        exists = true;
                        break;
                    }
                }
            }
            catch
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                    InsertOffering(offering);
                }
                catch { }
            }
            if (!exists)
            {
                var query2 =
                   from o in dc.Offerings
                   where o.OfferingID == offering.ParentID
                   select o;

                try
                {
                    foreach (Offering o in query2)
                    {
                        if (o.OfferingName.Equals("Product Guide"))
                            offering.Category = offering.OfferingName;
                        else
                            offering.Category = o.Category;
                    }
                    dc.Offerings.InsertOnSubmit(offering);
                    dc.SubmitChanges();
                }
                catch 
                {
                    RestartService(ref restartMssqlServiceAttempt);
                    dc = new ABBDataClassesDataContext();
                    try
                    {
                        dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                        InsertOffering(offering);
                    }
                    catch { }
                }
            }
        }

        public static int getOfferingID(int offeringID, int parentID)
        {
            int value = 0;

            var query =
                from o in dc.Offerings
                where o.OfferingID == offeringID && o.ParentID == parentID
                select o.OfferingID;
            try
            {
                foreach (int i in query)
                {
                    value = i;
                }
            }
            catch 
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Offerings);
                    return getOfferingID(offeringID, parentID);
                }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Attempts to insert a Category entity.
        /// </summary>
        /// <param name="category">The Category entity to insert.</param>
        public static void insertCategory(Category category)
        {
            if (!GetAllCategories().Contains(category.CategoryName))
            {
                try
                {
                    dc.Categories.InsertOnSubmit(category);
                    dc.SubmitChanges();
                }
                catch (Exception e)
                {
                    RestartService(ref restartMssqlServiceAttempt);
                    dc = new ABBDataClassesDataContext();
                    try
                    {
                        dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                        insertCategory(category);
                    }
                    catch (Exception ex)
                    {
                        /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                    }
                }
            }
        }
        /// <summary>
        /// Acquires a List of all unique category names.
        /// </summary>
        /// <returns>The List of unique category names.</returns>
        public static List<string> GetAllCategories()
        {
            List<string> categories = new List<string>();
            var query =
            (from d in dc.Categories
             orderby d.CategoryName
             select d.CategoryName).Distinct();

            int x = 0;
            try
            {
                foreach (string s in query)
                {
                    categories.Add(s);
                    x++;
                }
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                    return GetAllCategories();
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
            return categories;
        }
        /// <summary>
        /// Acquires a List of unique category names with specified offering name.
        /// </summary>
        /// <param name="offeringName">The offering name bound to the categories. Database property OfferingName.</param>
        /// <returns>The List of unique category names.</returns>
        public static List<string> GetCategories(int offeringID)
        {
            List<string> categories = new List<string>();
            var query =
            (from d in dc.Documents
             where d.OfferingID == offeringID
             orderby d.CategoryName
             select d.CategoryName).Distinct();

            int x = 0;
            try
            {
                foreach (string s in query)
                {
                    categories.Add(s);
                    x++;
                }
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Categories);
                    return GetCategories(offeringID);
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
            return categories;
        }
        /// <summary>
        /// Attempts to update a Tag entity with the specified offering name. 
        /// </summary>
        /// <param name="offeringName">The offering name bound to the Tag entity. Database property OfferingName.</param>
        /// <param name="tagID">The bytetag integer ID. This is equal to its hexadecimal value + 1. Database property TagID.</param>
        public static void UpdateTag(string offeringName, int offeringID, int tagID)
        {
            var query =
                from tag in dc.Tags
                where tag.TagID == tagID
                select tag;

            var oID =
                from offering in dc.Offerings
                where offering.OfferingID == offeringID
                select offering.OfferingID;
            try
            {
                foreach (Tag tag in query)
                {
                    if (oID.Count() == 0)
                    {
                        tag.OfferingID = null;
                    }
                    foreach (int i in oID)
                    {
                        tag.OfferingID = i;
                    }
                    tag.OfferingName = offeringName;
                    tag.IsEditable = true;
                }
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Tags);
                    UpdateTag(offeringName, offeringID, tagID);
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
        }
        /// <summary>
        /// Attempts to insert a Document entity.
        /// </summary>
        /// <param name="document">The Document entity to insert.</param>
        public static void InsertDocument(Document document)
        {
            var query1 =
                from o in dc.Offerings
                where o.OfferingName == document.OfferingName
                select o;

            var query2 =
                from c in dc.Categories
                where c.CategoryName == document.CategoryName
                select c;
            try
            {
                foreach (Offering o in query1)
                {
                    document.OfferingID = o.OfferingID;
                    foreach (Category c in query2)
                    {
                        document.CategoryID = c.CategoryID;
                    }
                }
                dc.Documents.InsertOnSubmit(document);
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Documents);
                    InsertDocument(document);
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
        }

        public static bool CheckCategoryExist(string categoryName)
        {
            bool exist = false;
            try
            {
                var query =
                    (from c in dc.Categories
                     where c.CategoryName.Equals(categoryName)
                     select c).First();
                if (query != null)
                {
                    exist = true;
                }
            }
            catch { }
            return exist;
        }

        public static bool CheckOfferingExist(int offeringID)
        {
            bool exist = false;
            try
            {
                var query =
                    (from o in dc.Offerings
                     where o.OfferingID == offeringID
                     select o).First();
                if (query != null)
                {
                    exist = true;
                }
            }
            catch { }
            return exist;
        }
        /// <summary>
        /// Attempts to delete a Category entity from the database. All documents bound to the Category must be deleted/changed first. 
        /// </summary>
        /// <param name="categoryName">The string name of the Category</param>
        public static void DeleteCategory(string categoryName)
        {
            if (CheckCategoryExist(categoryName))
            {
                try
                {
                    var query =
                        (from c in dc.Categories
                         where c.CategoryName.Equals(categoryName)
                         select c).FirstOrDefault();
                    Category category = (Category) query;
                    dc.Categories.DeleteOnSubmit(category);
                    dc.SubmitChanges();
                }
                catch (Exception e)
                { }
            }
        }


        /// <summary>
        /// Attempts to delete an Offering entity from the database. Also resets the Tag bound to it if there is one. All child elements must be deleted first. All files bound to it too. 
        /// </summary>
        /// <param name="offeringID">The int ID of the Offering.</param>
        public static void DeleteOffering(int offeringID)
        {
            try
            {
                
                var exist =
                    (from t in dc.Tags
                     where t.OfferingID == offeringID
                     select t).FirstOrDefault();
                if (exist != null)
                {
                    exist.OfferingID = null;
                    exist.OfferingName = null;
                }

                var deleteOffering =
                    (from o in dc.Offerings
                     where o.OfferingID == offeringID
                     select o).FirstOrDefault();
                Offering offering = (Offering) deleteOffering;
                dc.Offerings.DeleteOnSubmit(offering);
                dc.SubmitChanges();
            }
            catch (Exception e)
            { }
        }

        public static void InsertSetting(Setting setting)
        {
            try
            {
                var query =
                    (from s in dc.Settings
                     where s.SettingName.Equals(setting.SettingName)
                     select s).FirstOrDefault();
                if (query != null)
                {
                    query.RootFolder = setting.RootFolder;
                    query.Language = setting.Language;
                    query.FontSize = setting.FontSize;
                    query.LostTagTimeout = setting.LostTagTimeout;
                    query.OrientationOffsetFromTag = setting.OrientationOffsetFromTag;
                    query.BoxColorFrameThickness = setting.BoxColorFrameThickness;
                    query.CardColorFrameThickness = setting.CardColorFrameThickness;
                    query.CloseButtonSize = setting.CloseButtonSize;
                    query.DoubleTap = setting.DoubleTap;
                    query.BgTimer = setting.BgTimer;
                }
                
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Settings);
                    InsertSetting(setting);
                }
                catch { }
            }
        }

        public static void InsertBackgrounds(List<string> backgrounds)
        {
            const int chunkSize = 1024;
            byte[] byteArray = null;
            foreach (string path in backgrounds)
            {
                try
                {
                    int bytesRead;
                    int offset = 0;
                    var buffer = new byte[chunkSize];
                    using (System.IO.FileStream fis = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        byteArray = new byte[fis.Length];
                        while ((bytesRead = fis.Read(buffer, 0, chunkSize)) != 0)
                        {
                            System.Buffer.BlockCopy(buffer, 0, byteArray, offset, bytesRead);
                            offset += chunkSize;
                        }
                    }
                }
                catch (Exception e) { }

                try
                {
                    Background background;
                    background = new Background();
                    byte[] byteArrayBackground = byteArray;
                    background.BackgroundBinary = byteArrayBackground;
                    InsertBackground(background);
                }
                catch (Exception e) { }
            }
        }

        public static void InsertBackground(Background background)
        {
            try
            {
                dc.Backgrounds.InsertOnSubmit(background);
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Backgrounds);
                    InsertBackground(background);
                }
                catch { }
            }
        }

        /// <summary>
        /// Attempts to insert a Image entity.
        /// </summary>
        /// <param name="image">The Image entity to insert.</param>
        public static void InsertImage(Image image)
        {
            var query =
                from o in dc.Offerings
                where o.OfferingName == image.OfferingName
                select o;
            try
            {
                foreach (Offering o in query)
                {
                    image.OfferingID = o.OfferingID;
                }
                dc.Images.InsertOnSubmit(image);
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Images);
                    InsertImage(image);
                }
                catch (Exception ex)
                {
                    /*
                    using (var sc = new ServiceController("MSSQLSERVER"))
                        MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                    */
                }
            }
        }
        /// <summary>
        /// Attempts to insert a Video entity.
        /// </summary>
        /// <param name="video">The Video entity to insert.</param>
        public static void InsertVideo(Video video)
        {
            var query =
            from o in dc.Offerings
            where o.OfferingName == video.OfferingName
            select o;
            try
            {
                foreach (Offering o in query)
                {
                    video.OfferingID = o.OfferingID;
                }
                dc.Videos.InsertOnSubmit(video);
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                RestartService(ref restartMssqlServiceAttempt);
                dc = new ABBDataClassesDataContext();
                try
                {
                    dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                    InsertVideo(video);
                }
                catch (Exception ex)
                {
                    /*
                     using (var sc = new ServiceController("MSSQLSERVER"))
                         MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                     */
                }
            }
        }
        /// <summary>
        /// Checks if a title with specified entity filetype exists.
        /// </summary>
        /// <param name="title">The title of the entity. Database property Title.</param>
        /// <param name="fileType">The entity filetype. Options == {"Documents","Pictures","Videos"}</param>
        /// /// <param name="category">The category of the entity. Database property CategoryName</param>
        /// <returns>1 if true. 0 if false.</returns>
        public static int CheckIfTitleExist(string title, string fileType, string category)
        {
            int result = 0;
            switch (fileType)
            {
                case "Documents":
                    var query1 =
                        from d in dc.Documents
                        where d.Title == title
                        && d.CategoryName == category
                        select d;
                    foreach (Document d in query1)
                    {
                        if (d != null)
                            result = 1;
                    }
                    break;
                case "Pictures":
                    var query2 =
                        from i in dc.Images
                        where i.Title == title
                        select i;
                    foreach (Image i in query2)
                    {
                        if (i != null)
                            result = 1;
                    }
                    break;
                case "Videos":
                    var query3 =
                        from v in dc.Videos
                        where v.Title == title
                        select v;
                    foreach (Video v in query3)
                    {
                        if (v != null)
                            result = 1;
                    }
                    break;
            }
            return result;
        }

        public static int RestartService(ref int attempt)
        {
            attempt++;
            using (var sc = new ServiceController("MSSQLSERVER"))
            {
                try
                {
                    if (attempt >= 3)
                    {
                        foreach (var process in Process.GetProcessesByName("sqlservr"))
                        {
                            process.Kill();
                            process.WaitForExit(10000);
                        }
                        attempt = 1;
                        return attempt;
                    }
                }
                catch (Exception e) { }
                if (sc.Status.Equals(ServiceControllerStatus.Running))
                {
                    try
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(waitDuration));
                    }
                    catch (Exception e) { }
                    try
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(waitDuration));
                    }
                    catch (Exception e)
                    {
                        return RestartService(ref attempt);
                    }
                }
                else if (sc.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    try
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(waitDuration));
                    }
                    catch (System.ServiceProcess.TimeoutException e)
                    {
                        //
                    }
                    try
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(waitDuration));
                    }
                    catch (Exception e)
                    {
                        return RestartService(ref attempt);
                    }
                }
                else if (sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    try
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(waitDuration));
                    }
                    catch (System.ServiceProcess.TimeoutException e)
                    {
                        return RestartService(ref attempt);
                    }
                }
                else
                {
                    attempt = 1;
                    return attempt;
                }
            }
            attempt = 0;
            return attempt;
        }
        /// <summary>
        /// Attempts to delete an entity with the specified title and filetype.
        /// </summary>
        /// <param name="title">The title of the entity. Database property Title.</param>
        /// <param name="fileType">The entity filetype. Options == {"Documents","Pictures","Videos"}</param>
        public static bool DeleteFile(string title, string fileType)
        {
            bool status = true;
            switch (fileType)
            {
                case "Documents":
                    var query1 =
                        from d in dc.Documents
                        where d.Title == title
                        select d;
                    try
                    {
                        foreach (Document d in query1)
                        {
                            dc.Documents.DeleteOnSubmit(d);
                        }
                        dc.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        if (RestartService(ref restartMssqlServiceAttempt) > 0)
                            status = false;
                        dc = new ABBDataClassesDataContext();
                        try
                        {
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Documents);
                            DeleteFile(title, fileType);
                        }
                        catch (Exception ex)
                        {
                            /*
                             using (var sc = new ServiceController("MSSQLSERVER"))
                                 MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                             */
                            return false;
                        }
                    }
                    break;
                case "Pictures":
                    var query2 =
                        from i in dc.Images
                        where i.Title == title
                        select i;
                    try
                    {
                        foreach (Image i in query2)
                        {
                            dc.Images.DeleteOnSubmit(i);
                        }
                        dc.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        if (RestartService(ref restartMssqlServiceAttempt) > 0)
                            status = false;
                        dc = new ABBDataClassesDataContext();
                        try
                        {
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Images);
                            DeleteFile(title, fileType);
                        }
                        catch (Exception ex)
                        {
                            /*
                            using (var sc = new ServiceController("MSSQLSERVER"))
                                MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                            */
                            return false;
                        }
                    }
                    break;
                case "Videos":
                    var query3 =
                        from v in dc.Videos
                        where v.Title == title
                        select v;
                    try
                    {
                        foreach (Video v in query3)
                        {
                            dc.Videos.DeleteOnSubmit(v);
                        }
                        dc.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        if (RestartService(ref restartMssqlServiceAttempt) > 0)
                            status = false;
                        dc = new ABBDataClassesDataContext();
                        try
                        {
                            dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                            DeleteFile(title, fileType);
                        }
                        catch (Exception ex)
                        {
                            /*
                            using (var sc = new ServiceController("MSSQLSERVER"))
                                MainWindow.warningWindow.Show(sc.ServiceName + " is unresponsive. Please try again.");
                            */
                            return false;
                        }
                    }
                    break;
            }
            return status;
        }
        /// <summary>
        /// Attempts to delete an ObservableCollection of FileList objects.
        /// </summary>
        /// <param name="deleteOldFiles">The ObservableCollection of FileList objects to delete.</param>
        public static void DeleteFiles(ObservableCollection<FileList> deleteOldFiles)
        {
            foreach (var file in deleteOldFiles)
            {
                DeleteFile(file.Title, file.FileType);
            }
        }
        /// <summary>
        /// Attempts to insert an ObservableCollection of FileList objects.
        /// </summary>
        /// <param name="addNewFiles">The ObservableCollection of FileList objects to insert.</param>
        public static bool InsertFiles(ObservableCollection<FileList> addNewFiles)
        {
            bool status = true;
            byte[] byteArray = null;
            const int chunkSize = 1024;
            clone.Clear();
            List<string> fail = new List<string>();
            Video video = new Video();
            Document document = new Document();
            Image image = new Image();
            Background background = new Background();
            foreach (var file in addNewFiles)
            {
                try
                {
                    try
                    {
                        int bytesRead;
                        int offset = 0;
                        var buffer = new byte[chunkSize];
                        using (System.IO.FileStream fis = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read))
                        {
                            byteArray = new byte[fis.Length];
                            while ((bytesRead = fis.Read(buffer, 0, chunkSize)) != 0)
                            {
                                System.Buffer.BlockCopy(buffer, 0, byteArray, offset, bytesRead);
                                offset += chunkSize;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        fail.Add(file.Title.ToString());
                        clone.Add(file);
                    }


                    //if (CheckIfTitleExist(file.Title, file.FileType, file.CategoryType) == 0)// This check is done in ReprogramTask too, so no need to do it here. 
                    //{
                    if (byteArray != null)
                    {
                        switch (file.FileType)
                        {
                            case "Documents":
                                document = new Document();
                                document.OfferingName = file.OfferingName;
                                string fp = file.FilePath;
                                byte[] byteArrayDocument = byteArray;// = System.IO.File.ReadAllBytes(file.FilePath);
                                document.DocumentBinary = byteArrayDocument;
                                document.Title = file.Title;
                                document.CategoryName = file.CategoryType;
                                InsertDocument(document);
                                break;
                            case "Pictures":
                                image = new Image();
                                image.OfferingName = file.OfferingName;
                                byte[] byteArrayImage = byteArray;// = System.IO.File.ReadAllBytes(file.FilePath);
                                image.ImageBinary = byteArrayImage;
                                image.Title = file.Title;
                                InsertImage(image);
                                break;
                            case "Videos":
                                video = new Video();
                                video.OfferingName = file.OfferingName;
                                byte[] byteArrayVideo = byteArray;// = System.IO.File.ReadAllBytes(file.FilePath);
                                video.VideoBinary = byteArrayVideo;
                                video.Title = file.Title;
                                InsertVideo(video);
                                break;
                            case "Backgrounds":
                                background = new Background();
                                byte[] byteArrayBackground = byteArray;
                                background.BackgroundBinary = byteArrayBackground;
                                InsertBackground(background);
                                break;
                        }
                    }
                    //}
                }
                catch (Exception e)
                {
                    fail.Add(file.Title.ToString());
                    clone.Add(file);
                    if (RestartService(ref restartMssqlServiceAttempt) > 0)
                        status = false;
                    dc = new ABBDataClassesDataContext();
                    try
                    {
                        switch (file.FileType)
                        {
                            case "Documents":
                                dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Documents);
                                InsertDocument(document);
                                break;
                            case "Pictures":
                                dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Images);
                                InsertImage(image);
                                break;
                            case "Videos":
                                dc.Refresh(System.Data.Linq.RefreshMode.KeepChanges, dc.Videos);
                                InsertVideo(video);
                                break;
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            if (fail.Count > 0)
            {
                fail.Clear();
                insertAttempts++;
                if (insertAttempts <= 3)
                    InsertFiles(clone);
            }
            insertAttempts = 0;
            return status;
        }
    }
}
