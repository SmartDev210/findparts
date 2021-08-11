using Findparts.Models.WebApi;
using Findparts.Services.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Findparts.Controllers.AccountController;

namespace Findparts.Areas.WebApi.Controllers
{
    public class WebApiController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private readonly IWeavyService _chatService;
        private readonly IWebApiService _apiService;
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        public WebApiController(IWeavyService chatService, IWebApiService webApiService)
        {
            _signInManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationSignInManager>();

            _chatService = chatService;
            _apiService = webApiService;
        }
        
        [Route("mobile-api/auth")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MobileAuth()
        {
            ViewBag.ReturnUrl = Url.Action("MobileAuthCallback");
            return View("Login");
        }
        [Route("mobile-api/linkedin-auth")]        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult LinkedInAuth()
        {
            // Request a redirect to the external login provider
            return new ChallengeResult("LinkedIn", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = Url.Action("LinkedInCallback") }));
        }
        [Route("mobile-api/mobile-auth-callback")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MobileAuthCallback()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var email = User.Identity.GetEmail();

                var token = _chatService.GetWeavyToken(userId, email);

                return Redirect("elenaslist-mobile://#access_token=" + token);
            } else
            {
                return Redirect("elenaslist-mobile://#access_token=" + "fail");
            }
        }
        [Route("mobile-api/mobile-auth-logout")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MobileAuthAnother()
        {
            Session.Abandon();
            AuthenticationManager.SignOut();            
            return RedirectToAction("MobileAuth");
        }
        [Route("mobile-api/weavy-jwt")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> GetWeavyJwt(string email, string password)
        {
            string token = "";
            string message = "";
            SignInStatus result;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                result = SignInStatus.Failure;
            }
            else
            {
                result = await _signInManager.PasswordSignInAsync(email, password, false, false);
                var user = _signInManager.UserManager.FindByEmail(email);

                if (result == SignInStatus.Success && user.EmailConfirmed)
                {
                    token = _chatService.GetWeavyToken(user.Id, email);
                }
            }
            return Json(new
            {
                status = result.ToString(),
                message = message,
                token = token
            });
        }
        [Route("web-api/update-vendor")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateVendorFromWeavy(UpdateVendorFromWeavyRequest request)
        {   
            var user = _signInManager.UserManager.FindByEmail(request.UserEmail);
            if (user == null)
                return HttpNotFound();
            
            if (!_signInManager.UserManager.IsInRole(user.Id, "Vendor"))
            {
                _signInManager.UserManager.AddToRole(user.Id, "Vendor");
            }

            var result = _apiService.UpdateVendorFromWeavy(user, request);
            return Json(new { success = result });
        }
        [Route("web-api/add-vendor-user")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddVendorUser(AddVendorUserRequest request)
        {
            var user = _signInManager.UserManager.FindByEmail(request.CreatorEmail);
            if (user == null)
                return HttpNotFound();
            var added = _signInManager.UserManager.FindByEmail(request.Email);

            if (added == null)
                return HttpNotFound();
            if (_signInManager.UserManager.IsInRole(added.Id, "Vendor"))
            {
                _signInManager.UserManager.AddToRole(user.Id, "Vendor");
            }

            var result = _apiService.AddVendorUser(user, added);
            return Json(new { success = result });
        }
    }
}
