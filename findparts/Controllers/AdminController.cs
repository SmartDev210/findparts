using Findparts.Extensions;
using Findparts.Models.Admin;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _service;
        public AdminController(IAdminService service)
        {
            _service = service;
        }
        
        public ActionResult Vendors()
        {
            return View();
        }
        
        public ActionResult VendorList(int start = 0, int length = 100, int draw = 1)
        {
            if (!Request.IsAjaxRequest())
            {
                return HttpNotFound();
            }
            int colNum = 0;
            string sortData = "VendorName";
            string direction = "asc";
            string filter = "";
            if (Request.QueryString["order[0][column]"] != null)
            {
                colNum = Request.QueryString["order[0][column]"].ToNullableInt() ?? 0;
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                direction = Request.QueryString["order[0][dir]"];
            }

            if (Request.QueryString[$"columns[{colNum}][data]"] != null)
            {
                sortData = Request.QueryString[$"columns[{colNum}][data]"];
            }

            if (Request.QueryString["search[value]"] != null)
            {
                filter = Request.QueryString["search[value]"];
            }
            

            var result = _service.GetVendors(start, length, draw, sortData, direction, filter);
            return Json( new { data = result.Vendors, draw = result.Draw, recordsTotal = result.TotalRecords, recordsFiltered = result.FilteredRecords }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [Route("Admin/Vendors/{vendorId}")]
        public ActionResult VendorDetail(int vendorId)
        {
            VendorDetailViewModel viewModel = _service.GetVendorDetailViewModel(vendorId);

            return View("VendorDetail", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Vendors/{vendorId}")]
        public ActionResult VendorDetail(VendorDetailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel = _service.GetVendorDetailViewModel(viewModel.VendorId);
                return View("VendorDetail", viewModel);
            }

            _service.SaveVendorStatusAndNotes(viewModel);

            return RedirectToAction("Vendors", "Admin");
        }
    }
}