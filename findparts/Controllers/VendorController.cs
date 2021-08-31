using DAL;
using Findparts.ActionFilters;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Models.Subscriber;
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
        private readonly IMembershipService _membershipService;
        private readonly ISubscriberService _subscriberService;
        public VendorController(IVendorService vendorService, IMailService mailService, IMembershipService membershipService, ISubscriberService subscriberService)
        {
            _vendorService = vendorService;
            _mailService = mailService;
            _membershipService = membershipService;
            _subscriberService = subscriberService;
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

            VendorGeneralTabViewModel viewModel = _vendorService.GetVendorGeneralTabViewModel(vendorID);
            
            return View("~/Views/Vendor/GeneralTab.cshtml", viewModel);
        }
        public ActionResult RFQ()
        {
            string vendorID;
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorID = Request.QueryString["VendorID"];
            }
            else
            {
                vendorID = SessionVariables.VendorID;
            }
            Session["vendorID"] = vendorID;

            RFQPreferencesViewModel viewModel = _vendorService.GetRFQPreferencesViewModel(vendorID);

            return View("~/Views/Vendor/RFQ.cshtml", viewModel);
        }
        public ActionResult Advertise()
        {
            string vendorID;
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorID = Request.QueryString["VendorID"];
            }
            else
            {
                vendorID = SessionVariables.VendorID;
            }

            VendorAdvertiseViewModel viewModel = _vendorService.GetAdvertiseViewModel(vendorID);
            return View(viewModel);
        }

        public ActionResult DownloadInvoice(int chargeId)
        {
            string filepath = string.Format("{0}{1}.pdf", Config.InvoicePath, chargeId);
            if (!System.IO.File.Exists(filepath))
            {
                _vendorService.CreateInvoice(chargeId, true);
            }
            return File(filepath, "application/pdf", $"{Config.PortalName}-Invoice-{chargeId}.pdf");
        }
        [HttpGet]
        public ActionResult Purchase()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PurchaseImpressions()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Purchase(string stripeToken)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(SessionVariables.VendorID))
            {
                TempData["Error"] = "Something went wrong! Please try again";
                return RedirectToAction("Advertise");
            }

            
            if (_membershipService.PurchaseWithStripe(SessionVariables.VendorID, stripeToken))
            {
                TempData["Success"] = $"Successfully purchased";
            }
            
            return RedirectToAction("Advertise", "Vendor");
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PurchaseImpressions(string stripeToken, int quantity)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(SessionVariables.VendorID))
            {
                TempData["Error"] = "Something went wrong! Please try again";
                return RedirectToAction("Advertise");
            }

            if (_membershipService.PurchaseImpressionsWithStripe(SessionVariables.VendorID, stripeToken, quantity))
            {
                TempData["Success"] = $"Successfully purchased";
            }

            return RedirectToAction("Advertise", "Vendor");

        }
        public ActionResult Invoices()
        {
            if (Request.QueryString["Invoice"] != null && Request.QueryString["Id"] != null)
            {
                var filePath = _subscriberService.GetInvoice(Request.QueryString["Id"], (string)Session["subscriberID"]);
                return File(filePath, "application/pdf", "MROFINDER-Invoice.pdf");
            }

            string subscriberID;
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                subscriberID = Request.QueryString["SubscriberID"];
            }
            else
            {
                subscriberID = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberID;


            var subscriber = _membershipService.GetSubscriberById(subscriberID);
            var stripeCustomerId = subscriber.StripeCustomerID;
            var signupSubscriberTypeId = subscriber.SignupSubscriberTypeID;


            if (string.IsNullOrEmpty(stripeCustomerId))
            {
                SubscriberChargeViewModel viewModel = new SubscriberChargeViewModel
                {
                    SubscriptionUpdateMode = SubscriptionUpdateMode.Create
                };

                _subscriberService.PopulatePlanSelectList(viewModel);

                // new customer, show option to select plan and enter CC info

                if (signupSubscriberTypeId.HasValue)
                {
                    viewModel.SubscriberTypeId = signupSubscriberTypeId.Value;
                }

                return View(viewModel);
            }
            else
            {
                SubscriberChargeInfoViewModel viewModel = new SubscriberChargeInfoViewModel
                {
                    CardLastFour = "No card found",
                    PlanText = "No active plan",
                    SubscriptionStatus = "Not active",
                    SubscriptionPeriod = "Not active"
                };
                _subscriberService.PopulateSubscriberChargeInfoViewModel(viewModel, subscriber);

                return View("~/Views/Vendor/ChargeInfo.cshtml", viewModel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invoices(int? SubscriberTypeId, string stripeToken)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty((string)Session["subscriberID"]))
            {
                TempData["Error"] = "Something went wrong! Please try again";
                return RedirectToAction("Invoices");
            }
            var subscriber = _membershipService.GetSubscriberById((string)Session["subscriberID"]);

            if (string.IsNullOrEmpty(subscriber.StripeCustomerID) && SubscriberTypeId != null)
            {
                if (_membershipService.SubscribeWithStripe(SubscriberTypeId.Value, stripeToken, subscriber))
                {
                    SessionVariables.Populate();
                }
                TempData["Success"] = $"Subscription Successfully Added.<br><br>Welcome to {Config.PortalName}. Your Search Subscription is now active.";
            }
            else if (!string.IsNullOrEmpty(subscriber.StripeCustomerID))
            {
                // update/new card
                _membershipService.UpdateSubscribeWithStripe(stripeToken, subscriber);
                TempData["Success"] = "Payment Info Updated";
            }

            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                return RedirectToAction("Invoices", "Vendor", new { SubscriberID = Request.QueryString["SubscriberID"] });
            }
            else
            {
                return RedirectToAction("Invoices", "Vendor");
            }
        }
        [HttpPost]
        public ActionResult CancelCharge(string stripeSubscriptionId)
        {
            var subscriberId = (string)Session["subscriberID"];

            if (string.IsNullOrEmpty(subscriberId))
            {
                TempData["Error"] = "Someting went wrong! Plese try again";
                return RedirectToAction("Charge");
            }

            var subscriber = _membershipService.GetSubscriberById(subscriberId);
            if (subscriber == null) return Json(new { success = false });

            // check if cancellation is pending already
            if (subscriber.PendingSubscriberTypeID == ((int)SubscriberTypeID.NoCreditCard))
            {
                // already pending
            }
            else
            {
                // set pending cancellation
                if (_membershipService.CancelSubscription(subscriber, stripeSubscriptionId))
                {
                    TempData["Success"] = "Subscription Cancelled";
                }
            }
            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Vendor/Invoices/UpdatePlan")]
        public ActionResult UpdatePlan(int SubscriberTypeId)
        {
            var subscriberId = (string)Session["subscriberID"];
            var subscriber = _membershipService.GetSubscriberById(subscriberId);

            if (_membershipService.UpdateSubscriptionPlan(subscriber, SubscriberTypeId))
            {
                SessionVariables.Populate();
                TempData["Success"] = "Plan Updated";
            }
            else
            {
                TempData["Error"] = "Failed to update plan";
            }

            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                return RedirectToAction("Charge", new { SubscriberID = Request.QueryString["SubscriberID"] });
            }

            return RedirectToAction("Charge");
        }


        [HttpGet]
        [Route("Vendor/Invoices/UpdatePaymentInfo")]
        public ActionResult UpdateCharge()
        {
            string subscriberID;
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                subscriberID = Request.QueryString["SubscriberID"];
            }
            else
            {
                subscriberID = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberID;

            var subscriber = _membershipService.GetSubscriberById(subscriberID);
            if (string.IsNullOrEmpty(subscriber.StripeCustomerID))
            {
                return RedirectToAction("Invoices");
            }

            SubscriberChargeViewModel viewModel = new SubscriberChargeViewModel
            {
                SubscriptionUpdateMode = SubscriptionUpdateMode.UpdatePaymentInfo
            };
            return View("~/Views/Vendor/Invoices.cshtml", viewModel);
        }

        [HttpGet]
        [Route("Vendor/Invoices/UpdatePlan")]
        public ActionResult UpdatePlan()
        {
            string subscriberID;
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                subscriberID = Request.QueryString["SubscriberID"];
            }
            else
            {
                subscriberID = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberID;

            var subscriber = _membershipService.GetSubscriberById(subscriberID);
            if (string.IsNullOrEmpty(subscriber.StripeCustomerID))
            {
                return RedirectToAction("Charge");
            }

            SubscriberChargeViewModel viewModel = new SubscriberChargeViewModel
            {
                SubscriptionUpdateMode = SubscriptionUpdateMode.UpdatePlan
            };
            _subscriberService.PopulatePlanSelectList(viewModel);

            var cur = viewModel.PlanSelectList.FirstOrDefault(x => x.Value == subscriber.PendingSubscriberTypeID.ToString());
            viewModel.PlanSelectList.Remove(cur);

            return View("~/Views/Vendor/Invoices.cshtml", viewModel);
        }

        [HttpGet]
        public ActionResult Charge()
        {
            if (Request.QueryString["Invoice"] != null && Request.QueryString["Id"] != null)
            {
                var filePath = _subscriberService.GetInvoice(Request.QueryString["Id"], (string)Session["subscriberID"]);
                return File(filePath, "application/pdf", "MROFINDER-Invoice.pdf");
            }

            string subscriberID;
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                subscriberID = Request.QueryString["SubscriberID"];
            }
            else
            {
                subscriberID = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberID;


            var subscriber = _membershipService.GetSubscriberById(subscriberID);
            var stripeCustomerId = subscriber.StripeCustomerID;
            var signupSubscriberTypeId = subscriber.SignupSubscriberTypeID;


            if (string.IsNullOrEmpty(stripeCustomerId))
            {
                SubscriberChargeViewModel viewModel = new SubscriberChargeViewModel
                {
                    SubscriptionUpdateMode = SubscriptionUpdateMode.Create
                };

                _subscriberService.PopulatePlanSelectList(viewModel);

                // new customer, show option to select plan and enter CC info

                if (signupSubscriberTypeId.HasValue)
                {
                    viewModel.SubscriberTypeId = signupSubscriberTypeId.Value;
                }

                return View(viewModel);
            }
            else
            {
                SubscriberChargeInfoViewModel viewModel = new SubscriberChargeInfoViewModel
                {
                    CardLastFour = "No card found",
                    PlanText = "No active plan",
                    SubscriptionStatus = "Not active",
                    SubscriptionPeriod = "Not active"
                };
                _subscriberService.PopulateSubscriberChargeInfoViewModel(viewModel, subscriber);

                return View("~/Views/Vendor/ChargeInfo.cshtml", viewModel);
            }
        }
        public ActionResult Users()
        {
            string subscriberId;
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                subscriberId = Request.QueryString["SubscriberID"];
            }
            else
            {
                subscriberId = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberId;

            var viewModel = _subscriberService.GetUsersViewModel(subscriberId);

            return View(viewModel);
        }

        public ActionResult Oem()
        {
            string vendorID;
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorID = Request.QueryString["VendorID"];
            }
            else
            {
                vendorID = SessionVariables.VendorID;
            }
            Session["vendorID"] = vendorID;

            OEMsViewModel viewModel = _vendorService.GetOEMsViewModel(vendorID);

            return View("~/Views/Vendor/OEMsOnly.cshtml", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult General(VendorGeneralTabViewModel input)
        {
            TempData["VendorActiveTab"] = VendorActiveTab.GeneralTab;
            if (!ModelState.IsValid)
            {
                var viewModel = _vendorService.GetVendorGeneralTabViewModel(input.VendorId);
                return View("~/Views/Vendor/GeneralTab.cshtml", viewModel);
            }

            _vendorService.UpdateVendorGeneral(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = input.VendorId });
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
                var viewModel = _vendorService.GetRFQPreferencesViewModel(input.VendorId);
                return View("~/Views/Vendor/RFQ.cshtml", viewModel);
            }

            _vendorService.UpdateVendorRFQPrefs(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = input.VendorId });
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
                var viewModel = _vendorService.GetCertsViewModel(input.VendorId);
                return View("~/Views/Vendor/Certs.cshtml", viewModel);
            }

            _vendorService.AddVendorCert(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = input.VendorId });
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
                var viewModel = _vendorService.GetVendorGeneralTabViewModel(input.VendorId);
                return View("~/Views/Vendor/GenertalTab.cshtml", viewModel);
            }

            _vendorService.UpdateVendorAddress(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = input.VendorId });
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
                var viewModel = _vendorService.GetOEMsViewModel(input.VendorId);
                return View("~/Views/Vendor/OEMsOnly.cshtml", viewModel);
            }

            _vendorService.UpdateVendorOEM(input);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("VendorDetail", "Admin", new { vendorId = input.VendorId });
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
            string vendorId = "";
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            } else
            {
                vendorId = SessionVariables.VendorID;
            }

            VendorUploadListViewModel viewModel = _vendorService.GetVendorUploadListViewModel(vendorId);
            return View(viewModel);
        }        
        [HttpGet]
        [Route("Vendor/UploadCapability/{vendorListId?}")]
        public ActionResult UploadCapability(int? vendorListId)
        {
            string vendorId = "";
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            }
            else
            {
                vendorId = SessionVariables.VendorID;
            }

            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadCapabilityViewModel(vendorId, vendorListId ?? 0);
            return View(viewModel);
        }
        [HttpGet]
        [Route("Vendor/UploadAchievement")]
        public ActionResult UploadAchievement()
        {
            string vendorId = "";
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            }
            else
            {
                vendorId = SessionVariables.VendorID;
            }

            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadAchievementViewModel(vendorId);
            return View(viewModel);
        }

        [HttpGet]
        [Route("Vendor/UploadAchievement/{vendorAchievementId}")]
        public ActionResult UploadAchievement(int vendorAchievementId)
        {
            string vendorId = "";
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            }
            else
            {
                vendorId = SessionVariables.VendorID;
            }

            UploadVendorFileViewModel viewModel = _vendorService.GetVendorUploadAchievementViewModel(vendorId, vendorAchievementId);
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
                        return RedirectToAction("VendorDetail", "Admin", new { vendorId = viewModel.VendorId });
                    } else
                    {
                        TempData["Success"] = $@"List Successfully Added - <a href=""{Url.Action("Index", "Vendor")}"">View your Vendor Settings</a>";
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
        public ActionResult PostUploadAchievement(UploadVendorFileViewModel viewModel)
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
                        return RedirectToAction("VendorDetail", "Admin", new { vendorId = viewModel.VendorId });
                    }
                    else
                    {
                        TempData["Success"] = $@"List Successfully Added - <a href=""{Url.Action("Index", "Vendor")}"">View your Vendor Settings</a>";
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
            string vendorId = "";
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            }
            else
            {
                vendorId = SessionVariables.VendorID;
            }

            var vendorList = _vendorService.GetMasterVendorList(vendorId);

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

        public ActionResult Quote()
        {
            string vendorId = string.Empty;
            if (Request.QueryString["VendorID"] != null && User.IsInRole("Admin"))
            {
                vendorId = Request.QueryString["VendorID"];
            } else
            {
                vendorId = SessionVariables.VendorID;
            }

            if (Request.QueryString["VendorQuoteID"] != null)
            {
                string vendorQuoteIDInput = Request.QueryString["VendorQuoteID"];
                if (Request.QueryString["NoQuote"] != null)
                {
                    string[] vendorQuoteIDs;
                    if (vendorQuoteIDInput.Contains(","))
                    {
                        vendorQuoteIDs = vendorQuoteIDInput.Split(',');
                    } else
                    {
                        vendorQuoteIDs = new string[] { vendorQuoteIDInput };
                    }

                    foreach (var vendorQuoteId in vendorQuoteIDs)
                    {
                        _vendorService.VendorQuoteUpdateNoQuote(vendorQuoteId, vendorId);
                        _mailService.SendSubscriberQuoteEmail(vendorQuoteId);
                    }
                } else if (Request.QueryString["Ignore"] != null)
                {
                    string[] vendorQuoteIDs;
                    if (vendorQuoteIDInput.Contains(','))
                    {
                        vendorQuoteIDs = vendorQuoteIDInput.Split(',');
                    }
                    else
                    {
                        vendorQuoteIDs = new string[] { vendorQuoteIDInput };
                    }

                    foreach (string vendorQuoteID in vendorQuoteIDs)
                    {
                        _vendorService.VendorQuoteUpdateIgnore(vendorQuoteID, vendorId);
                    }
                } else
                {
                    string currency = Request.Form["currency"];
                    string testPrice = Request.Form["testPrice"];
                    string testTAT = Request.Form["testTAT"];
                    string repairPrice = Request.Form["repairPrice"];
                    string repairPriceRangeLow = Request.Form["repairPriceRangeLow"];
                    string repairPriceRangeHigh = Request.Form["repairPriceRangeHigh"];
                    string repairTAT = Request.Form["repairTAT"];
                    string overhaulPrice = Request.Form["overhaulPrice"];
                    string overhaulPriceRangeLow = Request.Form["overhaulPriceRangeLow"];
                    string overhaulPriceRangeHigh = Request.Form["overhaulPriceRangeHigh"];
                    string overhaulTAT = Request.Form["overhaulTAT"];
                    string notToExceed = Request.Form["notToExceed"];
                    bool repairsFrequently = Request.Form["repairsFrequently"] == "true";
                    bool pma = Request.Form["pma"] == "true";
                    bool der = Request.Form["der"] == "true";
                    bool freeEval = Request.Form["freeEval"] == "true";
                    bool modified = Request.Form["modified"] == "true";
                    bool functionTestOnly = Request.Form["functionTestOnly"] == "true";
                    bool noOverhaulWorkscope = Request.Form["noOverhaulWorkscope"] == "true";
                    bool caac = Request.Form["caac"] == "true";
                    bool extendedWarranty = Request.Form["extendedWarranty"] == "true";
                    bool flatRate = Request.Form["flatRate"] == "true";
                    bool range = !string.IsNullOrEmpty(repairPriceRangeLow) || !string.IsNullOrEmpty(repairPriceRangeHigh) || !string.IsNullOrEmpty(overhaulPriceRangeLow) || !string.IsNullOrEmpty(overhaulPriceRangeHigh);
                    bool nte = !string.IsNullOrEmpty(notToExceed);
                    string quoteComments = Request.Form["quoteComments"];

                    if (testPrice != "" || repairPrice != "" || overhaulPrice != "")
                    {
                        _vendorService.VendorQuoteUpdate(vendorQuoteIDInput, vendorId, currency, testPrice, testTAT, repairPrice, repairPriceRangeLow, repairPriceRangeHigh, repairTAT, overhaulPrice, overhaulPriceRangeLow, overhaulPriceRangeHigh, overhaulTAT, notToExceed, repairsFrequently, pma, der, freeEval, modified, functionTestOnly, noOverhaulWorkscope, caac, extendedWarranty, flatRate, range, nte, quoteComments);
                        
                        _mailService.SendSubscriberQuoteEmail(vendorQuoteIDInput);
                    }                    
                }
                return new EmptyResult();
            }

            var viewModel = _vendorService.GetVendorQuotesPageViewModel(vendorId);
            return View(viewModel);
        }
    }
}