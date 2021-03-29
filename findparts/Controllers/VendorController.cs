using Findparts.ActionFilters;
using Findparts.Core;
using Findparts.Models;
using Findparts.Models.Vendor;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult General(VendorGeneralTabViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.GeneralTab;
            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorIndexPageViewModel(input.VendorId);
                return View("~/Views/Vendor/Index.cshtml", viewModel);
            }

            _vendorService.UpdateVendorGeneral(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Vendors", "Admin", new { VendorID = input.VendorId });
            } else
            {
                return RedirectToAction("Index", "Vendor");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RFQUpdate(RFQPreferencesViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.RFQTab;
            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorIndexPageViewModel(input.VendorId);
                return View("~/Views/Vendor/Index.cshtml", viewModel);
            }

            _vendorService.UpdateVendorRFQPrefs(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Vendors", "Admin", new { VendorID = input.VendorId });
            }
            else
            {   
                return RedirectToAction("Index", "Vendor");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCert(CertsViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.CertsTab;

            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorIndexPageViewModel(input.VendorId);
                return View("~/Views/Vendor/Index.cshtml", viewModel);
            }

            _vendorService.AddVendorCert(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Vendors", "Admin", new { VendorID = input.VendorId });
            }
            else
            {   
                return RedirectToAction("Index", "Vendor");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAddress(AddressViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.AddressTab;

            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorIndexPageViewModel(input.VendorId);
                return View("~/Views/Vendor/Index.cshtml", viewModel);
            }

            _vendorService.UpdateVendorAddress(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Vendors", "Admin", new { VendorID = input.VendorId });
            }
            else
            {   
                return RedirectToAction("Index", "Vendor");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOEM(OEMsViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.OEMsOnlyTab;
            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorIndexPageViewModel(input.VendorId);
                return View("~/Views/Vendor/Index.cshtml", viewModel);
            }

            _vendorService.UpdateVendorOEM(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Vendors", "Admin", new { VendorID = input.VendorId });
            }
            else
            {   
                return RedirectToAction("Index", "Vendor");
            }
        }
        [HttpPost]
        public ActionResult DeleteCert(int certId)
        {
            _vendorService.DeleteVendorCert(certId);
            return Json(new { success = true });
        }
    }
}