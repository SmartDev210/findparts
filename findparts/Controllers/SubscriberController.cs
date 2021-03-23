﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models.Subscriber;
using Findparts.Services.Interfaces;
using Microsoft.AspNet.Identity.Owin;

namespace Findparts.Controllers
{
    public class SubscriberController : Controller
    {
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
        [Authorize]
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
        [Authorize]
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

            var userList = _service.GetUsersViewModel(subscriberId);

            return View(userList);
        }

        [Authorize]
        public ActionResult NewUser()
        {
            var subscriberId = (string)Session["subscriberID"];

            var viewModel = _service.GetSubscriberNewUserViewModel(subscriberId);

            return View(viewModel);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> NewUser(SubscriberNewUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var applicationUser = new Models.ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email
            };

            var result = await _userManager.CreateAsync(applicationUser);

            var subscriberId = (string)Session["subscriberID"];
            int? vendorId = null;
            if (result.Succeeded)
            {
                if (viewModel.VendorAdmin)
                {
                    vendorId = _service.GetVendorIdFromSubscriberId(subscriberId);
                }
            }
            if (vendorId == null)
            {
                await _userManager.AddToRoleAsync(applicationUser.Id, "Subscriber");
            } else
            {
                await _userManager.AddToRolesAsync(applicationUser.Id, "Subscriber", "Vendor");
            }

            
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = applicationUser.Id, code = code }, protocol: Request.Url.Scheme);

            _mailService.SendConfirmationEmail(applicationUser.Email, applicationUser.Email, callbackUrl, false, SessionVariables.Email);

            _mailService.SendAdminNewUserEmail(applicationUser.Email, SessionVariables.CompanyName, SessionVariables.Email);
            _mailService.SendSubscriberAdminNewUserEmail(SessionVariables.Email, applicationUser.Email, SessionVariables.CompanyName, SessionVariables.Email);


            _membershipService.UpdateUser(0, new Guid(applicationUser.Id), subscriberId.ToNullableInt(), vendorId, applicationUser.Email, SessionVariables.UserID.ToNullableInt());
            return RedirectToAction("Users");
        }

        [Authorize]
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

        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Charge(int? SubscriberTypeId, string stripeToken)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                return HttpNotFound();
            }
            var subscriber = _membershipService.GetSubscriberById((string)Session["subscriberID"]);

            if (string.IsNullOrEmpty(subscriber.StripeCustomerID) && SubscriberTypeId != null)
            {
                if (_membershipService.SubscribeWithStripe(SubscriberTypeId.Value, stripeToken, subscriber))
                {
                    SessionVariables.Populate();
                }
                TempData["Success"] = "Subscription Successfully Added.<br><br>Welcome to MRO FINDER. Your Search Subscription is now active.";
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
        [Authorize]
        public ActionResult CancelCharge(string stripeSubscriptionId)
        {
            var subscriberId = (string)Session["subscriberID"];
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
    }
}