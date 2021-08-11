using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Findparts.Models;
using Findparts.Core;
using Findparts.Services.Interfaces;
using DAL;
using Findparts.Extensions;
using JWT;
using JWT.Builder;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Findparts.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IMembershipService _membershipService;
        private readonly IMailService _mailService;
        private readonly IWeavyService _weavyService;

        public AccountController(IMembershipService membershipService, IMailService mailService, IWeavyService weavyService )
        {
            _userManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>(); ;
            _signInManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationSignInManager>();

            _membershipService = membershipService;
            _mailService = mailService;
            _weavyService = weavyService;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Signup.cshtml");
        }

        
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("mobile-auth"))
                {
                    TempData["Error"] = "Invalid login attempt.";
                    return RedirectToAction("MobileAuth", "WebApi");
                }
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    
                    SessionVariables.Populate(model.Email);
                    var user = _userManager.FindByEmail(model.Email);
                    if (!_userManager.IsInRole(user.Id, "Admin"))
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        RedirectToAction("Login");
                    }
                    if (!user.EmailConfirmed)
                    {
                        return RedirectToAction("VerifyEmail", new { returnUrl });
                    }

                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        if (SessionVariables.VendorID == "" && SessionVariables.SubscriberID != "")
                        {
                            var subscriber = _membershipService.GetSubscriberById(SessionVariables.SubscriberID);
                            if (subscriber != null)
                            {
                                if (subscriber.SignupSubscriberTypeID != (int)(SubscriberTypeID.NoCreditCard) && subscriber.SubscriberTypeID == (int)(SubscriberTypeID.NoCreditCard))
                                {
                                    return RedirectToAction("Charge", "Subscriber");
                                }
                            }
                        }
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("mobile-auth"))
                    {
                        TempData["Error"] = "Invalid login attempt.";
                        return RedirectToAction("MobileAuth", "WebApi");
                    }
                    
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await _signInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AdminLogin()
        {
            return View("~/Views/Account/Login.cshtml");
        }
        /*
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Signup()
        {   
            return View();
        }
        */
        /*
        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignUpWithEmail()
        {  
            return View();
        }
        */
        /*
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string Vendor, string SubscriberTypeID)
        {
            RegisterViewModel viewModel = new RegisterViewModel();

            _membershipService.PopulateRegisterViewModel(viewModel);

            if (Vendor.ToNullableInt() == null && SubscriberTypeID.ToNullableInt() == null)
                return RedirectToAction("Signup");

            if (Vendor == "1") viewModel.VendorSignup = true;
            viewModel.SubscriberTypeId = SubscriberTypeID.ToNullableInt();

            return View(viewModel);
        }
        */
        /*
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Session.Abandon();

                    _userManager.AddToRole(user.Id, "Subscriber");
                    if (model.VendorSignup)
                        _userManager.AddToRole(user.Id, "Vendor");

                    var vendorId = _membershipService.RegisterNewUser(model, user);

                    Session["RegisterVendorID"] = vendorId;
                    
                    await _signInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    _mailService.SendConfirmationEmail(user.Email, user.Email, callbackUrl);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("VerifyEmail");
                }
                AddErrors(result);
            }
            _membershipService.PopulateRegisterViewModel(model);
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        */
        [HttpGet]
        [Authorize]
        public ActionResult VerifyEmail(string returnUrl)
        {
            if (User.Identity.IsVerified())
            {
                return Redirect(returnUrl);
            }
            return View("ConfirmEmailSent");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SendVerificationEmail()
        {
            ApplicationUser user = null;
            if (User.IsInRole("Admin") && Request.Form["userId"] != null)
            {
                var appUser = _membershipService.GetUserById(Request.Form["userId"]);
                if (appUser != null)
                    user = _userManager.FindById(appUser.ProviderUserKey.ToString());
            } else
            {
                user = _userManager.FindById(User.Identity.GetUserId());
            }
            if (user == null)
            {
                return Json(new { success = false, errorMessage = "Unable to find user" });
            }
            if (user.EmailConfirmed)
            {
                return Json(new { success = false, errorMessage = "Already verified" });
            }

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

            if (_mailService.SendConfirmationEmail(user.Email, user.Email, callbackUrl))
            {
                return Json(new { success = true });
            } else
            {
                return Json(new { success = false, errorMessage = "Failed to send email" });
            }
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                await _signInManager.SignInAsync(user, true, true);

                _membershipService.ApproveUser(user, true);

                if (SessionVariables.VendorID != "")
                {
                    return RedirectToAction("UploadList", "Vendor");
                }
                if (SessionVariables.SubscriberID != "")
                {
                    // forward to ADD PLAN page. forever forward to ADD Plan page until CC added

                    var subscriber = _membershipService.GetSubscriberById(SessionVariables.SubscriberID);
                    if (subscriber != null)
                    {
                        if (subscriber.SignupSubscriberTypeID == (int)SubscriberTypeID.NoCreditCard)
                        {
                            // Check or wire, dont bug them
                            return RedirectToLocal("/");
                        }
                        if (subscriber.SubscriberTypeID == (int)SubscriberTypeID.NoCreditCard)
                        {
                            // no credit card on file, bug them
                            return RedirectToAction("Charge", "Subscriber");
                        }
                    }
                }

                return RedirectToLocal("~/");
            } else
            {
                return View("Error");
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed                    
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                 string code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                 var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                 _mailService.SendPasswordResetEmail(user.Email, callbackUrl);
                
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLoginFromBackChannel
        [HttpPost]
        [AllowAnonymous]       
        public ActionResult ExternalLoginFromBackChannel(string provider, string path)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallbackFromWeavy", "Account", new { Path = path }));
        }

        //
        // GET: /Account/ExternalLoginCallbackFromWeavy
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallbackFromWeavy(string path)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {  
                return Redirect($"{Config.WeavyUrl}/sign-in?path={path}");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalSignInAsync(loginInfo, isPersistent: true);
            switch (result)
            {
                case SignInStatus.Success:
                    Session.Abandon();
                    return Redirect($"{Config.WeavyUrl}/signing-in?path={path}&jwt={_weavyService.GetWeavyToken(null, loginInfo.Email)}");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = $"{Config.WeavyUrl}?path={path}", RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account

                    var user = new ApplicationUser { UserName = loginInfo.Email, Email = loginInfo.Email, EmailConfirmed = true };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        // create default Register view model
                        RegisterViewModel viewModel = new RegisterViewModel()
                        {
                            Email = loginInfo.Email,
                            VendorSignup = false,
                            AcceptTerm = true,
                            SubscriberTypeId = (int)SubscriberTypeID.NoCreditCard,
                            CompanyName = loginInfo.ExternalIdentity.Name,
                            Country = "United States"
                        };
                        Session.Abandon();

                        _userManager.AddToRole(user.Id, "Subscriber");

                        var vendorId = _membershipService.RegisterNewUser(viewModel, user);

                        Session["RegisterVendorID"] = vendorId;

                        var loginResult = await _userManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (loginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return Redirect($"{Config.WeavyUrl}/signing-in?path={path}&jwt={_weavyService.GetWeavyToken(null, loginInfo.Email)}");
                        }
                        //return View("ExternalLoginConfirmation");
                        //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
                    }
                    TempData["Error"] = "Failed to register linkedin user";                   
                    return Redirect($"{Config.WeavyUrl}/sign-in?path={path}");
            }
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await _signInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        
        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await _signInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }
        [Route("apple-signin")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AppleSigninCallback(string path)
        {
            string state = Request.Form["state"];
            var token = Request.Form["id_token"];
            try
            {
                JwtBase64UrlEncoder encoder = new JwtBase64UrlEncoder();
                var utfString =  Encoding.UTF8.GetString(encoder.Decode(token.Split('.')[1]));
                var jobj = JObject.Parse(utfString);
                if (jobj["iss"].ToString() == "https://appleid.apple.com" 
                    && jobj["aud"].ToString() == "com.elenaslist.elenasmobile.applesign"
                    && jobj["email_verified"].ToObject<bool>() == true)
                {
                    
                    var email = jobj["email"].ToString();
                    var user = _userManager.FindByEmail(email);
                    
                    var sub = jobj["sub"].ToString();
                    var loginInfo = new UserLoginInfo("apple", sub);

                    if (user != null)
                    {
                        var logins = _userManager.GetLogins(user.Id);
                        if (!logins.Any(x => x.LoginProvider == "apple"))
                        {
                            TempData["Error"] = "Email already exist!";
                        }
                        else
                        {
                            Session.Abandon();
                            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            if (state == "mobile")
                            {
                                return RedirectToAction("MobileAuthCallback", "WebApi");
                            } else if (state == "backchannel")
                            {
                                var jwtToken = _weavyService.GetWeavyToken(user.Id, user.Email);
                                return Redirect($"{Config.WeavyUrl}/signing-in?path={path}&jwt={jwtToken}");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {
                        user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                        var createResult = await _userManager.CreateAsync(user);
                        if (createResult.Succeeded)
                        {
                            // create default Register view model
                            RegisterViewModel viewModel = new RegisterViewModel()
                            {
                                Email = email,
                                VendorSignup = false,
                                AcceptTerm = true,
                                SubscriberTypeId = (int)SubscriberTypeID.NoCreditCard,
                                CompanyName = email,
                                Country = "United States"
                            };
                            Session.Abandon();

                            _userManager.AddToRole(user.Id, "Subscriber");

                            var vendorId = _membershipService.RegisterNewUser(viewModel, user);

                            Session["RegisterVendorID"] = vendorId;

                            var loginResult = await _userManager.AddLoginAsync(user.Id, loginInfo);
                            if (loginResult.Succeeded)
                            {
                                await _signInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                                if (state == "mobile")
                                {   
                                    return RedirectToAction("MobileAuthCallback", "WebApi");
                                }
                                else if (state == "backchannel")
                                {
                                    var jwtToken = _weavyService.GetWeavyToken(user.Id, user.Email);
                                    return Redirect($"{Config.WeavyUrl}/signing-in?path={path}&jwt={jwtToken}");
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            TempData["Error"] = "Failed to sign in.";
                        }

                    }
                }
            } catch (Exception ex)
            {
                TempData["Error"] = "Failed to sign in using apple";
            }

            if (state == "mobile")
            {
                return RedirectToAction("MobileAuth", "WebApi");
            } else if(state == "backchannel")
            {
                return Redirect($"{Config.WeavyUrl}/sign-in?path={path}");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {   
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                TempData["Error"] = "Failed to login";
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("mobile-api"))
                    return RedirectToAction("MobileAuth", "WebApi");
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalSignInAsync(loginInfo, isPersistent: true);
            switch (result)
            {
                case SignInStatus.Success:
                    Session.Abandon();
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    
                    var user = new ApplicationUser { UserName = loginInfo.Email, Email = loginInfo.Email, EmailConfirmed = true };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        // create default Register view model
                        RegisterViewModel viewModel = new RegisterViewModel()
                        {
                            Email = loginInfo.Email,
                            VendorSignup = false,
                            AcceptTerm = true,
                            SubscriberTypeId = (int)SubscriberTypeID.NoCreditCard,
                            CompanyName = loginInfo.ExternalIdentity.Name,
                            Country = "United States"
                        };
                        Session.Abandon();

                        _userManager.AddToRole(user.Id, "Subscriber");

                        var vendorId = _membershipService.RegisterNewUser(viewModel, user);

                        Session["RegisterVendorID"] = vendorId;
                       
                        var loginResult = await _userManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (loginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToLocal(returnUrl);
                        }
                        //return View("ExternalLoginConfirmation");
                        //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
                    }
                    TempData["Error"] = "Failed to register linkedin user";
                    if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("mobile-api"))
                        return RedirectToAction("MobileAuth", "WebApi");
                    return RedirectToAction("Login");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> ExternalRegister(string Vendor, string SubscriberTypeID, string ReturnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            ExternalLoginConfirmationViewModel viewModel = new ExternalLoginConfirmationViewModel
            {
                Email = loginInfo.Email
            };

            _membershipService.PopulateRegisterViewModel(viewModel);

            if (Vendor.ToNullableInt() == null && SubscriberTypeID.ToNullableInt() == null)
                return RedirectToAction("Signup");

            if (Vendor == "1") viewModel.VendorSignup = true;
            viewModel.SubscriberTypeId = SubscriberTypeID.ToNullableInt();

            ViewBag.ReturnUrl = ReturnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            return View("ExternalLoginConfirmation", viewModel);
        }
        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // create default Register view model
                    RegisterViewModel viewModel = new RegisterViewModel()
                    {
                        Email = model.Email,
                        Country = model.Country,
                        PhoneNumber = model.PhoneNumber,                        
                        VendorSignup = model.VendorSignup,
                        AcceptTerm = true,
                        CompanyName = model.Email,

                    };
                    Session.Abandon();

                    _userManager.AddToRole(user.Id, "Subscriber");
                    if (viewModel.VendorSignup)
                        _userManager.AddToRole(user.Id, "Vendor");

                    var vendorId = _membershipService.RegisterNewUser(viewModel, user);

                    Session["RegisterVendorID"] = vendorId;

                    result = await _userManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpGet]
        // [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Abandon();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);            
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            var cnt = await _membershipService.DeleteUser(userId);

            return Json(new { success = (cnt > 0) });
        }
        [Authorize]
        [HttpPost]
        public ActionResult SendResetPasswordLink(string userId)
        {
            var user = _membershipService.GetUserById(userId);
            if (user != null)
            {
                var appUser = _userManager.FindByEmail(user.Email);
                if (appUser != null)
                {
                    var code = _userManager.GeneratePasswordResetToken(appUser.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = appUser.Id, code = code }, protocol: Request.Url.Scheme);
                    
                    _mailService.SendPasswordResetEmail(user.Email, callbackUrl);
                    return Json(new { success = true });
                }
                    
            }
            

            return Json(new { success = false });
        }
    }
}