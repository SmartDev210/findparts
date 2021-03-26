using Findparts.ActionFilters;
using Findparts.Core;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    [Authorize(Roles ="Vendor, Admin")]
    [RequireEmailConfirmed]
    public class VendorController : Controller
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // GET: Vendor
        public ActionResult Index()
        {
            string vendorID;
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorID = Request.QueryString["VendorID"];
            } else
            {
                vendorID = SessionVariables.VendorID;
            }
            Session["vendorID"] = vendorID;

            var viewModel = _vendorService.GetVendorIndexPageViewModel(vendorID);
            return View(viewModel);
        }
    }
}