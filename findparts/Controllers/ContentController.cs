using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    public class ContentController : Controller
    {   
        [HttpGet]
        [Route("about")]
        public ActionResult About()
        {
            return View("~/Views/Content/About.cshtml");
        }

        [HttpGet]
        [Route("terms")]
        public ActionResult Terms()
        {
            return View("~/Views/Content/Terms.cshtml");
        }

        [HttpGet]
        [Route("privacy")]
        public ActionResult Privacy()
        {
            return View("~/Views/Content/Privacy.cshtml");
        }

        [HttpGet]
        [Route("contact")]
        public ActionResult Contact()
        {
            return View("~/Views/Content/Contact.cshtml");
        }
    }
}