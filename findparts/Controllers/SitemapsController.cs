using Findparts.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    public class SitemapsController : Controller
    {
        public SitemapsController()
        {

        }
        [AllowAnonymous]
        [Route("sitemaps/{filename}")]
        public ActionResult Sitemap(string filename)
        {
            var path = Path.Combine(Config.SitemapPath, Config.PortalCode.ToString(), filename);
            if (System.IO.File.Exists(path))
            {
                return new FilePathResult(path, "text/xml");
            }
            return HttpNotFound();
        }
    }
}