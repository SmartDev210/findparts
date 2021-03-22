using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
       
    }
}