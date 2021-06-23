using Findparts.Core;
using Findparts.Helpers;
using Findparts.Models;
using Findparts.Services.Interfaces;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    public class VideoCallController : Controller
    {
        private readonly IJitsiService _jitsiService;

        public VideoCallController(IJitsiService jitsiService)
        {
            _jitsiService = jitsiService;
        }

        [HttpGet]
        [Authorize(Roles = "Subscriber, Admin")]
        [Route("video-call/{vendorId}")]
        // GET: VideoCall
        public ActionResult Index(int vendorId)
        {
            var token = JaaSJwtBuilder.Builder()
                .WithDefaults()
                .WithApiKey(Config.JitsiApiKey)
                //.WithUserName(User.Identity.Name)
                //.WithUserEmail(User.Identity.GetEmail())
                .WithAppID(Config.JitsiAppId)
                .Encode();
            

            var callbackUrl = Url.Action("VideoCall", "VideoCall", new { vendorId, userId = User.Identity.GetUserId() }, protocol: Request.Url.Scheme);
            _jitsiService.SendInvitiationEmails(vendorId, User.Identity.GetEmail(), callbackUrl);
            VideoCallViewModel viewModel = new VideoCallViewModel
            {
                VendorId = vendorId,
                Token = token,
                UserEmail = User.Identity.GetEmail(),
                RoomName = $"room-{vendorId}-{User.Identity.GetUserId()}"
            };
            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Subscriber, Vendor, Admin")]
        [Route("video-call/{vendorId}/{userId}")]
        // GET: VideoCall
        public ActionResult VideoCall(int vendorId, string userId)
        {
            
            List<string> vendorUsers = _jitsiService.GetVendorUserList(vendorId);
            if (User.IsInRole("Admin")) { }
            else if (User.IsInRole("Vendor") && vendorUsers.Contains(User.Identity.GetEmail())) {

            } 
            else if (User.IsInRole("Subscriber") && userId.ToLower() == User.Identity.GetUserId().ToLower()) {
                
            } else {
                return RedirectToAction("Index", "Home");
            }
            var token = JaaSJwtBuilder.Builder()
               .WithDefaults()
               .WithApiKey(Config.JitsiApiKey)
               //.WithUserName(User.Identity.Name)
               //.WithUserEmail(User.Identity.GetEmail())
               .WithAppID(Config.JitsiAppId)
               .Encode();
            VideoCallViewModel viewModel = new VideoCallViewModel
            {
                VendorId = vendorId,
                Token = token,
                UserEmail = User.Identity.GetEmail(),
                RoomName = $"room-{vendorId}-{userId}"
            };
            return View("Index", viewModel);
            
        }
    }
}