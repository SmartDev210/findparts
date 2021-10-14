using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DAL;
using Findparts.ActionFilters;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Models.Subscriber;
using Findparts.Services.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Findparts.Controllers
{
    [Authorize]
    [RequireEmailConfirmed]
    public class SubscriberController : Controller
    {
        /*
        private readonly ISubscriberService _service;
        private ApplicationUserManager _userManager;
        private readonly IMailService _mailService;
        private readonly IMembershipService _membershipService;

        public SubscriberController(ISubscriberService service, IMailService mailService, IMembershipService membershipService)
        {
            _service = service;
            _userManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>();
            _mailService = mailService;
            _membershipService = membershipService;
        }
        
        // GET: Subscriber
        public ActionResult Index()
        {
            string subscriberId;
            if (User.IsInRole("Admin") && Request.QueryString["SubscriberID"] != null)
            {
                subscriberId = Request.QueryString["SubscriberID"];
            } else
            {
                subscriberId = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberId;

            var viewModel = _service.GetSubscriberIndexPageViewModel(subscriberId.ToNullableInt());
            return View(viewModel);
        }
        
        public ActionResult Users()
        {
            string subscriberId; 
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin")) {
                subscriberId = Request.QueryString["SubscriberID"];
            } else
            {
                subscriberId = SessionVariables.SubscriberID;
            }
            Session["subscriberID"] = subscriberId;

            var viewModel = _service.GetUsersViewModel(subscriberId);

            return View(viewModel);
        }

        
        public ActionResult NewUser()
        {
            var subscriberId = (string)Session["subscriberID"];

            var viewModel = _service.GetSubscriberNewUserViewModel(subscriberId);

            return View(viewModel);
        }

        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NewUser(SubscriberNewUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var user = _userManager.FindByEmail(viewModel.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Please let user sign up first");
                return View(viewModel);
            }

            var subscriberId = (string)Session["subscriberID"];
            int? vendorId = null;
          
            if (viewModel.VendorAdmin)
            {
                vendorId = _service.GetVendorIdFromSubscriberId(subscriberId);
            }
          
            if (vendorId != null)
            {
                await _userManager.AddToRolesAsync(user.Id, "Vendor");
            }

            
            _mailService.SendAdminNewUserEmail(user.Email, SessionVariables.CompanyName, SessionVariables.Email);
            _mailService.SendSubscriberAdminNewUserEmail(SessionVariables.Email, user.Email, SessionVariables.CompanyName, SessionVariables.Email);


            _membershipService.UpdateUser(0, new Guid(user.Id), subscriberId.ToNullableInt(), vendorId, user.Email, SessionVariables.UserID.ToNullableInt());
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Users", new { SubscriberID = subscriberId });
            } else 
                return RedirectToAction("Users");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Subscriber/Charge/UpdatePlan")]
        public ActionResult UpdatePlan(int SubscriberTypeId)
        {
            var subscriberId = (string)Session["subscriberID"];
            var subscriber = _membershipService.GetSubscriberById(subscriberId);

            if (_membershipService.UpdateSubscriptionPlan(subscriber, SubscriberTypeId))
            {
                SessionVariables.Populate();
                TempData["Success"] = "Plan Updated";
            } else
            {
                TempData["Error"] = "Failed to update plan";
            }

            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {
                return RedirectToAction("Charge", new { SubscriberID = Request.QueryString["SubscriberID"]});
            }

            return RedirectToAction("Charge");
        }

        
        [HttpGet]
        [Route("Subscriber/Charge/UpdatePaymentInfo")]
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
                return RedirectToAction("Charge");
            }

            SubscriberChargeViewModel viewModel = new SubscriberChargeViewModel
            {
                SubscriptionUpdateMode = SubscriptionUpdateMode.UpdatePaymentInfo
            };
            return View("~/Views/Subscriber/Charge.cshtml", viewModel);
        }
        
        [HttpGet]
        [Route("Subscriber/Charge/UpdatePlan")]
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
            _service.PopulatePlanSelectList(viewModel);

            var cur = viewModel.PlanSelectList.FirstOrDefault(x => x.Value == subscriber.PendingSubscriberTypeID.ToString());
            viewModel.PlanSelectList.Remove(cur);

            return View("~/Views/Subscriber/Charge.cshtml", viewModel);
        }
        
        [HttpGet]
        public ActionResult Charge()
        {
            if (Request.QueryString["Invoice"] != null && Request.QueryString["Id"] != null)
            {
                var filePath = _service.GetInvoice(Request.QueryString["Id"], (string)Session["subscriberID"]);
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

                _service.PopulatePlanSelectList(viewModel);

                // new customer, show option to select plan and enter CC info

                if (signupSubscriberTypeId.HasValue)
                {
                    viewModel.SubscriberTypeId = signupSubscriberTypeId.Value;
                }

                return View(viewModel);
            } else
            {
                SubscriberChargeInfoViewModel viewModel = new SubscriberChargeInfoViewModel
                {
                    CardLastFour = "No card found",
                    PlanText = "No active plan",
                    SubscriptionStatus = "Not active",
                    SubscriptionPeriod = "Not active"
                };
                _service.PopulateSubscriberChargeInfoViewModel(viewModel, subscriber);

                return View("~/Views/Subscriber/ChargeInfo.cshtml", viewModel);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Charge(int? SubscriberTypeId, string stripeToken)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty((string)Session["subscriberID"]))
            {
                TempData["Error"] = "Something went wrong! Please try again";
                return RedirectToAction("Charge");
            }
            var subscriber = _membershipService.GetSubscriberById((string)Session["subscriberID"]);

            if (string.IsNullOrEmpty(subscriber.StripeCustomerID) && SubscriberTypeId != null)
            {
                if (_membershipService.SubscribeWithStripe(SubscriberTypeId.Value, stripeToken, subscriber))
                {
                    SessionVariables.Populate();
                }
                TempData["Success"] = $"Subscription Successfully Added.<br><br>Welcome to {Config.PortalName}. Your Search Subscription is now active.";
            } else if (!string.IsNullOrEmpty(subscriber.StripeCustomerID))
            {
                // update/new card
                _membershipService.UpdateSubscribeWithStripe(stripeToken, subscriber);
                TempData["Success"] = "Payment Info Updated";
            }
            
            if (Request.QueryString["SubscriberID"] != null && User.IsInRole("Admin"))
            {   
                return RedirectToAction("Charge", "Subscriber", new { SubscriberID = Request.QueryString["SubscriberID"] });
            }
            else
            {
                return RedirectToAction("Charge", "Subscriber");
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
            if (subscriber == null) return Json(new { success = false});

            // check if cancellation is pending already
            if (subscriber.PendingSubscriberTypeID == ((int)SubscriberTypeID.NoCreditCard))
            {
                // already pending
            } else
            {
                // set pending cancellation
                if (_membershipService.CancelSubscription(subscriber, stripeSubscriptionId))
                {
                    TempData["Success"] = "Subscription Cancelled";
                }
            }
            return Json(new { success = true });
        }

        
        [HttpGet]        
        public ActionResult Address()
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

            AddressViewModel viewModel = _service.GetAddressPageViewModel(subscriberID);
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAddress(AddressViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Subscriber/Address.cshtml", viewModel);
            }

            var subscriberId = (string)Session["subscriberID"];
            if (string.IsNullOrEmpty(subscriberId))
            {
                TempData["Error"] = "Something went wrong! Please try again";
                return RedirectToAction("Address");
            }
            if (_service.UpdateSubscriberAddress(subscriberId, viewModel))
            {
                TempData["Success"] = "Address updated";
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Subscribers", "Admin", new { SubscriberID = subscriberId });
                return RedirectToAction("Address");
            } else
            {
                ModelState.AddModelError("", "Failed to update address");                
            }
            return View("~/Views/Subscriber/Address.cshtml", viewModel);
        }
        
        [HttpGet]
        
        public ActionResult PreferredBlocked()
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
            bool blocked = false;
            if (Request.QueryString["Block"] != null)
                blocked = true;
            SubscriberVendorsPageViewModel viewModel = _service.GetSubscriberVendorsPageViewModel(subscriberID, blocked);

            return View("~/Views/Subscriber/Vendors.cshtml", viewModel);
        }
        
        [HttpPost]
        public ActionResult UndoPreferBlock(VendorsPageMode Mode, int VendorId)
        {
            if (string.IsNullOrEmpty((string)Session["subscriberID"]))
            {
                return Json(new { success = false });
            }
            if (_service.UndoPreferBlock(Mode, VendorId, (string)Session["subscriberID"]))
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpGet]
        public ActionResult Quote()
        {
            var viewModel = _service.GetQuotePageViewModel(SessionVariables.UserID);

            return View(viewModel);
        }
        */
    }
}