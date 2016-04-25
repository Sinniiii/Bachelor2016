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

namespace SmartCardManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        [FromServices]
        public Models.ABBDataContext context { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        //POST for deleting dataitem from a smartcard
        [HttpPost]
        public async Task<IActionResult> OverviewDeleteDataitem(int tagID, int dataItemID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            var dataItemToRemove = await context.SmartCardDataItems.FirstAsync(a => a.Id == dataItemID);
            context.SmartCardDataItems.Remove(dataItemToRemove);
            await context.SaveChangesAsync();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }

        //POST for changing name of smartcard
        [HttpPost]
        public async Task<IActionResult> OverviewSaveSmartcardName(string nameOfCard, int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            var smartcard = await context.SmartCards.FirstAsync(s => s.TagId == tagID);

            smartcard.Name = nameOfCard;
            await context.SaveChangesAsync();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });
        }


        [HttpPost]
        public async Task<IActionResult> OverviewUploadDataitem(IFormFile uploadfile, int tagID)
        {

            System.Diagnostics.Debug.WriteLine("-------RUNNING UPLOAD----------");

            var fileName = ContentDispositionHeaderValue.Parse(uploadfile.ContentDisposition).FileName.Trim('"');
            var extension = fileName.Substring(Math.Max(0, fileName.Length - 3));

            var readstream = uploadfile.OpenReadStream();
            byte[] bytes;
            bytes = new byte[readstream.Length];  //declare arraysize
            readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array

            System.Diagnostics.Debug.WriteLine("-------Extension was----------");
            System.Diagnostics.Debug.WriteLine(extension);

            SmartCardDataItem item1;

            //TODO PROPER INPUT CONTROL
            //Create new dataitem
            if (extension == "pdf")
            {
                System.Diagnostics.Debug.WriteLine("-------Creating document category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Document, bytes);
            }
            else if (extension == "mp4")
            {
                System.Diagnostics.Debug.WriteLine("-------Creating video category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Video, bytes);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("-------Creating image category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Image, bytes);
            }

            //Add to smartcard based on tagID

            var cardToUpdate = await context.SmartCards.Include(s => s.DataItems).FirstAsync(a => a.TagId == tagID);
            cardToUpdate.DataItems.Add(item1);

            await context.SaveChangesAsync();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return RedirectToAction("Overview", new { tagID = tagID });

        }

        [HttpPost]
        public async Task<IActionResult> OverviewUploadDataitemDZ(ICollection<IFormFile> files, int tagID)
        {


            var uploadfile = Request.Form.Files.FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("-------RUNNING UPLOAD----------");

            var fileName = ContentDispositionHeaderValue.Parse(uploadfile.ContentDisposition).FileName.Trim('"');
            var extension = fileName.Substring(Math.Max(0, fileName.Length - 3));

            var readstream = uploadfile.OpenReadStream();
            byte[] bytes;
            bytes = new byte[readstream.Length];  //declare arraysize
            readstream.Read(bytes, 0, bytes.Length); // read from stream to byte array

            System.Diagnostics.Debug.WriteLine("-------Extension was----------");
            System.Diagnostics.Debug.WriteLine(extension);

            SmartCardDataItem item1;

            //TODO PROPER INPUT CONTROL
            //Create new dataitem
            if (extension == "pdf")
            {
                System.Diagnostics.Debug.WriteLine("-------Creating document category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Document, bytes);
            }
            else if (extension == "mp4")
            {
                System.Diagnostics.Debug.WriteLine("-------Creating video category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Video, bytes);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("-------Creating image category----------");
                item1 = new SmartCardDataItem(fileName, SmartCardDataItemCategory.Image, bytes);
            }

            //Add to smartcard based on tagID
            //ABBDataContext context = new ABBDataContext();
            //var smartcardList = context.SmartCards.OrderBy(a => a.TagId).ToList();
            var cardToUpdate = await context.SmartCards.Include(s => s.DataItems).FirstAsync(a => a.TagId == tagID);
            cardToUpdate.DataItems.Add(item1);

            await context.SaveChangesAsync();

            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

            return View("Overview", context.SmartCards.ToList());

        }

        ////General Overview GET
        //[HttpGet]
        //public IActionResult Overview()
        //{
        //    System.Diagnostics.Debug.WriteLine("-------RUNNING GET----------");

        //    ABBDataContext context = new ABBDataContext();
        //    var smartcardList = context.SmartCards.OrderBy(a => a.TagId).ToList();
        //    var x = context.SmartCardDataItems;

        //    //var firstname = smartcardList[0].Name;

        //    //ViewData["SmartcardList"] = smartcardList.Count;

        //    ViewData["tagID"] = -1;
        //    ViewData["activePanel"] = "paneltitle_" + -1;

        //    return View(smartcardList);
        //}

        //General Overview GET, with tagID
        [HttpGet]
        public IActionResult Overview(int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING GET----------");
            
            var smartcardList = context.SmartCards.Include(s => s.DataItems).OrderBy(a => a.TagId).ToList();
            
            ViewData["tagID"] = tagID;
            ViewData["activePanel"] = "paneltitle_" + tagID;

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
