using Findparts.ActionFilters;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Models.Vendor;
using Findparts.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private readonly IMailService _mailService;
        public VendorController(IVendorService vendorService, IMailService mailService)
        {
            _vendorService = vendorService;
            _mailService = mailService;
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

        [HttpGet]
        public ActionResult UploadList()
        {
            VendorUploadListViewModel viewModel = _vendorService.GetVendorUploadListViewModel(SessionVariables.VendorID);
            return View(viewModel);
        }        
        [HttpGet]
        [Route("Vendor/UploadCapability/{vendorListId?}")]
        public ActionResult UploadCapability(int? vendorListId)
        {
            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadCapabilityViewModel(SessionVariables.VendorID, vendorListId ?? 0);
            return View(viewModel);
        }
        [HttpGet]
        public ActionResult UploadAchievement()
        {
            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadAchievementViewModel(SessionVariables.VendorID);
            return View(viewModel);
        }

        [HttpGet]
        [Route("Vendor/UploadAchievement/{vendorAchievementId}")]
        public ActionResult UploadAchievement(int vendorAchievementId)
        {
            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadAchievementViewModel(SessionVariables.VendorID, vendorAchievementId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        [Route("Vendor/UploadCapability/{id?}")]
        public ActionResult UploadCapability(UploadVendorFileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.Upload != null && viewModel.Upload.ContentLength > 0)
            {
                string fileType = "." + viewModel.Upload.FileName.Split('.').Last().ToLower();
                if (fileType == ".xlsx" || fileType == ".xls" || fileType == ".csv")
                {
                    _vendorService.UploadVendorList(viewModel, fileType);

                    if (!User.IsInRole("Admin"))
                    {
                        _mailService.SendVendorUploadEmail(SessionVariables.Email, SessionVariables.CompanyName, true);
                    }
                    _mailService.SendAdminUploadEmail(SessionVariables.CompanyName, viewModel.VendorId.ToString(), true);

                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Vendors", "Admin", new { VendorID = viewModel.VendorId });
                    } else
                    {
                        TempData["Success"] = $@"List Successfully Added - <a href=""{Url.Action("Index", "Vendor")}"">View your Repair Station Settings</a>";
                    }
                } else
                {
                    ModelState.AddModelError("Upload", "Only the following filetypes are supported: .xlsx, .xls, .csv");
                    return View(viewModel);
                }
            } else if (viewModel.Id == 0)
            {
                ModelState.AddModelError("Upload", "Please select file");
                return View(viewModel);
            } else
            {
                _vendorService.UploadVendorList(viewModel, null);
            }

            return RedirectToAction("UploadList");
        }
        [HttpPost]
        [Route("Vendor/UploadAchievement/{id?}")]        
        [ValidateAntiForgeryToken]
        public ActionResult UploadAchievement(UploadVendorFileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.Upload != null && viewModel.Upload.ContentLength > 0)
            {
                string fileType = "." + viewModel.Upload.FileName.Split('.').Last().ToLower();
                if (fileType == ".xlsx" || fileType == ".xls" || fileType == ".csv")
                {
                    _vendorService.UploadVendorAchievement(viewModel, fileType);

                    if (!User.IsInRole("Admin"))
                    {
                        _mailService.SendVendorUploadEmail(SessionVariables.Email, SessionVariables.CompanyName, false);
                    }
                    _mailService.SendAdminUploadEmail(SessionVariables.CompanyName, viewModel.VendorId.ToString(), false);

                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Vendors", "Admin", new { VendorID = viewModel.VendorId });
                    }
                    else
                    {
                        TempData["Success"] = $@"List Successfully Added - <a href=""{Url.Action("Index", "Vendor")}"">View your Repair Station Settings</a>";
                    }
                }
                else
                {
                    ModelState.AddModelError("Upload", "Only the following filetypes are supported: .xlsx, .xls, .csv");
                    return View(viewModel);
                }
            } else if (viewModel.Id == 0)
            {
                ModelState.AddModelError("Upload", "Please select file");
                return View(viewModel);
            } else
            {
                _vendorService.UploadVendorAchievement(viewModel, null);
            }

            return RedirectToAction("UploadList");
        }
        [HttpGet]
        public ActionResult DownloadVendorList(int vendorListId)
        {
            string fileName = _vendorService.GetVendorListFileName(vendorListId);
            if (string.IsNullOrEmpty(fileName))
                return HttpNotFound();

            return File(Path.Combine(Config.UploadPath, fileName), fileName.GetContentType(), fileName);
        }
        [HttpGet]
        public ActionResult DownloadAchievement(int vendorAchievementId)
        {
            string fileName = _vendorService.GetVendorAchievementFileName(vendorAchievementId);
            if (string.IsNullOrEmpty(fileName))
                return HttpNotFound();

            return File(Path.Combine(Config.UploadPath, fileName), fileName.GetContentType(), fileName);
        }
        [HttpGet]
        public ActionResult DownloadMasterList()
        {
            var vendorList = _vendorService.GetMasterVendorList(SessionVariables.VendorID);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            MemoryStream memoryStream = new MemoryStream();

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("CapList");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromCollection(vendorList, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:Z1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }
                
                pck.SaveAs(memoryStream);                
            }
            memoryStream.Position = 0;
            return File(memoryStream, ".xlsx".GetContentType(), "MasterList.xlsx");
        }

        [HttpGet]
        public ActionResult DeleteVendorList(int vendorListId)
        {
            _vendorService.DeleteVendorList(vendorListId);
            TempData["Success"] = "List Successfullly Deleted";
            return RedirectToAction("UploadList");
        }
        [HttpGet]
        public ActionResult DeleteAchievement(int vendorAchievementId)
        {
            _vendorService.DeleteVendorAchievement(vendorAchievementId);
            TempData["Success"] = "List Successfullly Deleted";
            return RedirectToAction("UploadList");
        }
    }
}