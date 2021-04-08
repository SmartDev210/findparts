using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models.Admin;
using Findparts.Services.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpGet]
        [Route("Admin/PreviewCapability/{vendorListId}")]        
        public ActionResult PreviewCapability(int vendorListId)
        {
            var vendorList = _service.GetVendorList(vendorListId);

            if (vendorList == null) {
                TempData["Error"] = "Invalid Vendor List";
                return RedirectToAction("Vendors", "Admin");
            }

            var filePath = Path.Combine(Config.UploadPath, $"{vendorListId}{vendorList.Filetype}");

            var items = _service.LoadDataFromExcelFile<VendorListItem>(filePath);
            if (items == null)
            {
                TempData["Error"] = "Invalid file";
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorList.VendorID });
            }
            
            return View("PreviewCapability", new PreviewCapabilityViewModel { Items = items, Preview = true });
        }
        [HttpGet]
        [Route("Admin/ApproveCapability/{vendorListId}")]
        public ActionResult ApproveCapability(int vendorListId)
        {
            var vendorList = _service.GetVendorList(vendorListId);

            if (vendorList == null)
            {
                TempData["Error"] = "Invalid Vendor List";
                return RedirectToAction("Vendors", "Admin");
            }

            var filePath = Path.Combine(Config.UploadPath, $"{vendorListId}{vendorList.Filetype}");

            var items = _service.LoadDataFromExcelFile<VendorListItem>(filePath);
            if (items == null)
            {
                TempData["Error"] = "Invalid file";
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorList.VendorID });
            }

            return View("PreviewCapability", new PreviewCapabilityViewModel { Items = items, Preview = false, VendorListId = vendorListId });
        }

        [HttpGet]
        [Route("Admin/ImportCapability/{vendorListId}")]
        public ActionResult ImportCapability(int vendorListId)
        {
            var vendorList = _service.GetVendorList(vendorListId);

            if (vendorList == null)
            {
                TempData["Error"] = "Invalid Vendor List";
                return RedirectToAction("Vendors", "Admin");
            }

            try
            {
                string message;
                _service.ImportVendorList(vendorList, out message);

                TempData["Success"] = "Vendor list imported";
                TempData["Message"] = message;
            } catch (Exception ex)
            {
                TempData["Error"] = "Failed to import <br/>" + ex.Message + "<br/>" + ex.StackTrace;
            }
            
            return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorList.VendorID });
        }
        [HttpGet]
        [Route("Admin/PrevewAchievement/{vendorAchievementListId}")]
        public ActionResult PrevewAchievement(int vendorAchievementListId)
        {
            var vendorAchievementList = _service.GetVendorAchievementList(vendorAchievementListId);

            if (vendorAchievementList == null)
            {
                TempData["Error"] = "Invalid Vendor Achievement List";
                return RedirectToAction("Vendors", "Admin");
            }

            var filePath = Path.Combine(Config.UploadPath, $"Achievement_{vendorAchievementList.VendorAchievementListID}{vendorAchievementList.Filetype}");

            var items = _service.LoadDataFromExcelFile<VendorAchievementListItem>(filePath);
            if (items == null)
            {
                TempData["Error"] = "Invalid file";
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorAchievementList.VendorID });
            }

            return View("PreviewAchievement", new PreviewAchievementViewModel { Items = items, Preview = true });
        }

        [HttpGet]
        [Route("Admin/ApproveAchievement/{vendorAchievementListId}")]
        public ActionResult ApproveAchievement(int vendorAchievementListId)
        {
            var vendorAchievementList = _service.GetVendorAchievementList(vendorAchievementListId);

            if (vendorAchievementList == null)
            {
                TempData["Error"] = "Invalid Vendor Achievement List";
                return RedirectToAction("Vendors", "Admin");
            }

            var filePath = Path.Combine(Config.UploadPath, $"Achievement_{vendorAchievementList.VendorAchievementListID}{vendorAchievementList.Filetype}");

            var items = _service.LoadDataFromExcelFile<VendorAchievementListItem>(filePath);
            if (items == null)
            {
                TempData["Error"] = "Invalid file";
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorAchievementList.VendorID });
            }

            return View("PreviewAchievement", new PreviewAchievementViewModel { Items = items, Preview = false, VendorAchievementListId = vendorAchievementListId });
        }

        [HttpGet]
        [Route("Admin/ImportAchievement/{vendorAchievementListId}")]
        public ActionResult ImportAchievement(int vendorAchievementListId)
        {
            var vendorAchievementList = _service.GetVendorAchievementList(vendorAchievementListId);

            if (vendorAchievementList == null)
            {
                TempData["Error"] = "Invalid Vendor Achievement List";
                return RedirectToAction("Vendors", "Admin");
            }

            try
            {
                string message;
                _service.ImportVendorAchievementList(vendorAchievementList, out message);

                TempData["Success"] = "Vendor achievement list imported";
                TempData["Message"] = message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to import <br/>" + ex.Message + "<br/>" + ex.StackTrace;
            }

            return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorAchievementList.VendorID });
        }

        [HttpGet]
        [Route("Admin/DeleteVendorList/{vendorListId}")]
        public ActionResult DeleteVendorList(int vendorListId)
        {
            var vendorList = _service.GetVendorList(vendorListId);

            if (vendorList == null)
                TempData["Error"] = "Failed to delete vendor list";
            else
            {
                try
                {
                    _service.DeleteVendorList(vendorListId);
                    TempData["Success"] = "Vendor list deleted";
                } catch (Exception ex)
                {
                    TempData["Error"] = "Faield to delete vendor list <br/>" + ex.Message + "<br/>" + ex.StackTrace;
                }
                
            }
            return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorList.VendorID });
        }
        [HttpGet]
        [Route("Admin/DeleteAchievementList/{vendorAchievementListId}")]
        public ActionResult DeleteAchievementList(int vendorAchievementListId)
        {
            var vendorAchievementList = _service.GetVendorAchievementList(vendorAchievementListId);

            if (vendorAchievementList == null)
                TempData["Error"] = "Failed to delete vendor list";
            else
            {
                try
                {
                    _service.DeleteAchievementList(vendorAchievementListId);
                    TempData["Success"] = "Vendor achievement list deleted";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Faield to delete vendor list <br/>" + ex.Message + "<br/>" + ex.StackTrace;
                }

            }
            return RedirectToAction("VendorDetail", "Admin", new { vendorId = vendorAchievementList.VendorID });
        }
    }
}