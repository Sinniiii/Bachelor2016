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

        public IActionResult Overview()
        {
            //ABBDataContext context = new ABBDataContext();
            //var smartcardListTask = context.SmartCards.ToListAsync();
            //while (!smartcardListTask.IsCompleted)
            //{
            //    //wait
            //}
            //var smartcardList = smartcardListTask.Result;
            //var firstname = smartcardList[0].Name;
            return View();
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
