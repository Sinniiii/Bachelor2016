using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DatabaseModel;
using Microsoft.Data.Entity;

namespace SmartCardManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Overview(string nameOfCard, int tagID)
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING POST----------");

            ABBDataContext context = new ABBDataContext();
            var smartcardList = context.SmartCards.OrderBy(a => a.TagId).ToList();
            var x = context.SmartCardDataItems;

            var cardToUpdate = smartcardList.Find(a => a.TagId == tagID);
            cardToUpdate.Name = nameOfCard;
            context.SaveChanges();

            //DEBUG
            //System.Diagnostics.Debug.WriteLine(nameOfCard);
            //ViewData["nameOfCard"] = nameOfCard;
            //ViewData["tagID"] = tagID;

            return View(smartcardList);
        }

        [HttpGet]
        public IActionResult Overview()
        {
            System.Diagnostics.Debug.WriteLine("-------RUNNING GET----------");

            ABBDataContext context = new ABBDataContext();
            var smartcardList = context.SmartCards.OrderBy(a => a.TagId).ToList();
            var x = context.SmartCardDataItems;

            //var firstname = smartcardList[0].Name;

            //ViewData["SmartcardList"] = smartcardList.Count;

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
