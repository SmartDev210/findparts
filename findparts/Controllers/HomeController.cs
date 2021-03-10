using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace findparts.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.BodyClass = "home-page";
            return View();
        }
    }
}