﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DatabaseModel;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Http;
using System.IO;
using DatabaseModel.Model;
using Microsoft.Net.Http.Headers;

//asdadd

namespace SmartCardManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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

            if(dataItemToRemove.Category == SmartCardDataItemCategory.Video && System.IO.File.Exists(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + dataItemToRemove.Name) )
            {
                System.IO.File.Delete(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + dataItemToRemove.Name);
            }
            
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
                if (v.Category == SmartCardDataItemCategory.Video && System.IO.File.Exists(SmartCardDataItem.VIDEO_FOLDER + tagID + @"\" + v.Name) )
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

        //General Overview GET, with tagID
        [HttpGet]
        public IActionResult Overview([FromQuery]int tagID = -1)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING GET----------");

            ABBDataContext context = new ABBDataContext();
            var smartcardList = context.SmartCards.OrderByDescending(a => a.DataItems.Count).ThenBy(a => a.TagId).ToList();

            var firstEmptyIndex = -1;
            var tagIsActive = false;

            //Find the first empty card (by index)
            for (int i = 0; i <= smartcardList.Count; i++)
            {
                var count = smartcardList.ElementAt(i).DataItems.Count;
                var currenttagID = smartcardList.ElementAt(i).TagId;
                if (tagID == currenttagID)
                {
                    tagIsActive = true;
                }

                if (count == 0)
                {
                    firstEmptyIndex = i;

                    break;
                }
            }

            if (tagID < 0) { tagIsActive = true; }

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

        public IActionResult Contact()
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
