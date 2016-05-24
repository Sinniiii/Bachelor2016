using System.Linq;
using Microsoft.AspNet.Mvc;
using DatabaseModel;
using Microsoft.AspNet.Http;
using System.IO;
using DatabaseModel.Model;
using Microsoft.Net.Http.Headers;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SmartCardManagementSystem.Controllers
{
    public class HomeController : Controller
    {

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        /// RESIZE IMAGE
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // CRATE IMAGE FROM BYTE ARRAY
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        // CREATE BYTE ARRAY FROM IMAGE
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }




        public IActionResult Index()
        {
            //return View();
            return RedirectToAction("Overview");
        }

        //POST for deleting dataitem from a smartcard
        [HttpPost]
        public IActionResult OverviewDeleteDataitem(int tagID, int dataItemID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            ABBDataContext context = new ABBDataContext();
            var dataItemToRemove = context.SmartCardDataItems.First(a => a.Id == dataItemID);
            context.Entry(dataItemToRemove).Reference(a => a.DataField).Load();
            context.SmartCardDataItems.Remove(dataItemToRemove);
            context.SaveChanges();

            if (dataItemToRemove.Category == SmartCardDataItemCategory.Video && System.IO.File.Exists(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + dataItemToRemove.Name))
            {
                System.IO.File.Delete(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + dataItemToRemove.Name);
            }

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }

        //POST for deleting cardimage from a smartcard
        [HttpPost]
        public IActionResult OverviewDeleteCardimage(int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            ABBDataContext context = new ABBDataContext();
            var smartCard = context.SmartCards.First(a => a.TagId == tagID);
            context.Entry(smartCard).Reference(a => a.CardImage).Load();
            var cardimageToRemove = smartCard.CardImage;
            context.Entry(cardimageToRemove).Reference(a => a.DataField).Load();
            context.SmartCardImages.Remove(cardimageToRemove);
            context.SaveChanges();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }

        //POST for changing name of smartcard
        [HttpPost]
        public IActionResult OverviewSaveSmartcardName(string nameOfCard, int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            ABBDataContext context = new ABBDataContext();
            var smartcard = context.SmartCards.First(s => s.TagId == tagID);

            smartcard.Name = nameOfCard;
            context.SaveChanges();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }

        //POST for deleting all dataitems on card
        [HttpPost]
        public IActionResult OverviewDeleteAllDataitems(int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST - delete all dataitems----------");

            ABBDataContext context = new ABBDataContext();
            //var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
            //var cardToUpdate = context.SmartCards.Include(s => s.DataItems.Select(d => d.DataField)).FirstOrDefault(a => a.TagId == tagID);
            //cardToUpdate.DataItems.Clear();

            var list = context.SmartCardDataItems.Where(a => a.SmartCard.TagId == tagID);

            foreach (var v in list)
            {
                context.Entry(v).Reference(a => a.DataField).Load();
                if (v.Category == SmartCardDataItemCategory.Video && System.IO.File.Exists(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + v.Name))
                {
                    System.IO.File.Delete(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + v.Name);
                }
            }

            context.SmartCardDataItems.RemoveRange(list);

            context.SaveChanges();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }

        //OLD UPLOAD CONTROLLER BEFORE IMPLMENTING DROPZONE - DEACTIVATED
        //[HttpPost]
        //public IActionResult OverviewUploadDataitem(IFormFile uploadfile, int tagID)
        //{

        //    System.Diagnostics.Debug.WriteLine("-------RUNNING UPLOAD----------");




        //    var fileName = ContentDispositionHeaderValue.Parse(uploadfile.ContentDisposition).FileName.Trim('"');
        //    int lastPeriod = fileName.LastIndexOf('.');
        //    string extension = fileName.Substring(lastPeriod + 1);
        //    //var extension = fileName.Substring(Math.Max(0, fileName.Length - 3));

        //    System.Diagnostics.Debug.WriteLine("-------Extension was----------");
        //    System.Diagnostics.Debug.WriteLine(extension);

        //    SmartCardDataItem item1;

        //    //TODO PROPER INPUT CONTROL
        //    //Create new dataitem
        //    if (extension == "pdf")
        //    {
        //        var readstream = uploadfile.OpenReadStream();
        //        byte[] bytes = new byte[readstream.Length];  //declare arraysize
        //        readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array

        //        System.Diagnostics.Debug.WriteLine("-------Creating document category----------");
        //        item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Document, bytes);

        //        //Add to smartcard based on tagID
        //        ABBDataContext context = new ABBDataContext();
        //        var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
        //        cardToUpdate.DataItems.Add(item1);

        //        context.SaveChanges();
        //    }
        //    //3gp 3g2 asx avi mp4 mpeg avi mov uvu tts wtv dvr-ms wm wmv wmx 
        //    else if (extension == "3gp"
        //        || extension == "3g2"
        //        || extension == "avi"
        //        || extension == "mp4"
        //        || extension == "mpeg"
        //        || extension == "mpg"
        //        || extension == "avi"
        //        || extension == "mov"
        //        || extension == "wm"
        //        || extension == "wmv"
        //        || extension == "wmx"
        //        )
        //    {
        //        System.Diagnostics.Debug.WriteLine("-------Creating video category----------");

        //        // Special stuff for making video items
        //        item1 = new SmartCardDataItem(fileName);

        //        //Add to smartcard based on tagID
        //        ABBDataContext context = new ABBDataContext();
        //        var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
        //        cardToUpdate.DataItems.Add(item1);

        //        context.SaveChanges();

        //        uploadfile.SaveAs(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + fileName);
        //    }
        //    //bmp gif png jpg svg tiff dds wdp emf ico wmf
        //    else if (extension == "bmp"
        //        || extension == "gif"
        //        || extension == "png"
        //        || extension == "jpg"
        //        || extension == "svg"
        //        || extension == "tiff"
        //        || extension == "ico"
        //        || extension == "wmf"
        //        )
        //    {
        //        var readstream = uploadfile.OpenReadStream();
        //        byte[] bytes = new byte[readstream.Length];  //declare arraysize
        //        readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array

        //        System.Diagnostics.Debug.WriteLine("-------Creating image category----------");
        //        item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Image, bytes);

        //        //Add to smartcard based on tagID
        //        ABBDataContext context = new ABBDataContext();
        //        var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
        //        cardToUpdate.DataItems.Add(item1);

        //        context.SaveChanges();
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine("-------INVALID FILE TYPE, DO NOTHING----------");
        //    }

        //    ViewData["tagID"] = tagID;
        //    ViewData["activePanel"] = "paneltitle_" + tagID;

        //    return RedirectToAction("Overview", new { tagID = tagID });

        //}

        //Upload function using DropZone for dataitems
        [HttpPost]
        public IActionResult OverviewUploadDataitemDZ(int tagID)
        {

            var uploadfile = Request.Form.Files.FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("-------RUNNING UPLOAD----------");

            var fileName = ContentDispositionHeaderValue.Parse(uploadfile.ContentDisposition).FileName.Trim('"');
            int lastPeriod = fileName.LastIndexOf('.');
            string extension = fileName.Substring(lastPeriod + 1);

            System.Diagnostics.Debug.WriteLine("-------Extension was----------");
            System.Diagnostics.Debug.WriteLine(extension);

            SmartCardDataItem item1;

            //TODO PROPER INPUT CONTROL
            //Create new dataitem
            if (extension == "pdf")
            {
                var readstream = uploadfile.OpenReadStream();
                byte[] bytes = new byte[readstream.Length];  //declare arraysize
                readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array
                readstream.Close();

                System.Diagnostics.Debug.WriteLine("-------Creating document category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Document, bytes);

                ABBDataContext context = new ABBDataContext();
                var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
                cardToUpdate.DataItems.Add(item1);



                context.SaveChanges();
            }
            //3gp 3g2 asx avi mp4 mpeg avi mov uvu tts wtv dvr-ms wm wmv wmx 
            else if (extension == "3gp"
                || extension == "3g2"
                || extension == "avi"
                || extension == "mp4"
                || extension == "mpeg"
                || extension == "mpg"
                || extension == "avi"
                || extension == "mov"
                || extension == "wm"
                || extension == "wmv"
                || extension == "wmx"
                )
            {
                System.Diagnostics.Debug.WriteLine("-------Creating video category----------");

                // Special stuff for making video items
                item1 = new SmartCardDataItem(fileName);

                System.IO.Directory.CreateDirectory(SmartCardDataItem.VIDEO_FOLDER + tagID);
                uploadfile.SaveAs(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + fileName);

                ABBDataContext context = new ABBDataContext();
                var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
                cardToUpdate.DataItems.Add(item1);

                context.SaveChanges();
            }
            else if (extension == "bmp"
                || extension == "gif"
                || extension == "png"
                || extension == "jpg"
                || extension == "tiff"
                || extension == "ico"
                )
            {
                var readstream = uploadfile.OpenReadStream();
                byte[] bytes = new byte[readstream.Length];  //declare arraysize
                readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array
                readstream.Close();

                System.Diagnostics.Debug.WriteLine("-------Creating image category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Image, bytes);

                ABBDataContext context = new ABBDataContext();
                var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
                cardToUpdate.DataItems.Add(item1);

                context.SaveChanges();
            }

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            //return View("Overview", context.SmartCards.Include(a => a.DataItems).ToList());
            return new EmptyResult();

        }

        //Upload function using DropZone for cardimage
        [HttpPost]
        public IActionResult OverviewUploadDataitemDZcardimage(int tagID)
        {

            var uploadfile = Request.Form.Files.FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("-------RUNNING UPLOAD----------");

            var fileName = ContentDispositionHeaderValue.Parse(uploadfile.ContentDisposition).FileName.Trim('"');
            int lastPeriod = fileName.LastIndexOf('.');
            string extension = fileName.Substring(lastPeriod + 1);

            System.Diagnostics.Debug.WriteLine("-------Extension was----------");
            System.Diagnostics.Debug.WriteLine(extension);

            //Create new dataitem
            if (extension == "bmp"
                || extension == "gif"
                || extension == "png"
                || extension == "jpg"
                || extension == "tiff"
                || extension == "ico"
                )
            {
                var readstream = uploadfile.OpenReadStream();
                byte[] bytes = new byte[readstream.Length];  //declare arraysize
                readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array
                readstream.Close();

                var fileSize = bytes.Count();
                System.Diagnostics.Debug.WriteLine("filesize was: " + fileSize);

                if (fileSize > 100000) //if file larger than 100KB - resize then save
                {
                    System.Diagnostics.Debug.WriteLine("-------Creating RESIZED front-image (more than 100KB)----------");

                    var tempImageToResize = byteArrayToImage(bytes);
                    var tempResizedImage = ResizeImage(tempImageToResize, 180, 111);
                    var resizedByteArray = imageToByteArray(tempResizedImage);
                    var item1 = new SmartCardImage(fileName, resizedByteArray);

                    ABBDataContext context = new ABBDataContext();
                    var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
                    var oldimage = cardToUpdate.CardImage;
                    cardToUpdate.CardImage = item1;

                    context.SaveChanges();
                }
                else  //if file smaller than 100KB - just save it
                {
                    System.Diagnostics.Debug.WriteLine("-------Creating UNTOUCHED front-image (less than 100KB)----------");

                    var item1 = new SmartCardImage(fileName, bytes);

                    ABBDataContext context = new ABBDataContext();
                    var cardToUpdate = context.SmartCards.First(a => a.TagId == tagID);
                    var oldimage = cardToUpdate.CardImage;
                    cardToUpdate.CardImage = item1;

                    context.SaveChanges();
                }
            }

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            //return View("Overview", context.SmartCards.Include(a => a.DataItems).ToList());
            return new EmptyResult();

        }

        //General Overview GET, with tagID
        [HttpGet]
        public IActionResult Overview([FromQuery]int tagID = -1)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING GET----------");

            ABBDataContext context = new ABBDataContext();
            var smartcardList = context.SmartCards.OrderByDescending(a => a.DataItems.Count > 0).ThenBy(a => a.TagId).ToList();

            var firstEmptyIndex = -1;
            var tagIsActive = false;

            //Find the first empty card (by index)
            for (int i = 0; i <= smartcardList.Count; i++)
            {
                var count = smartcardList.ElementAt(i).DataItems.Count;
                var currenttagID = smartcardList.ElementAt(i).TagId;

                if (count == 0)
                {
                    firstEmptyIndex = i;

                    break;
                }

                if (tagID == currenttagID || tagID == -1)
                {
                    //System.Diagnostics.Debug.WriteLine("DEEEEEBUUUUUUGGG " +tagID);
                    //System.Diagnostics.Debug.WriteLine("DEEEEEBUUUUUUGGG " + i);
                    tagIsActive = true;
                }  
            }

            //Set viewdata
            ViewData["tagID"] = tagID;
            ViewData["tagIsActive"] = tagIsActive;
            ViewData["activePanel"] = "paneltitle_" + tagID;
            ViewData["firstEmptyIndex"] = firstEmptyIndex;

            return View(smartcardList);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Help()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
