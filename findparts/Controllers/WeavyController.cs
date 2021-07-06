using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Findparts.Services.Interfaces;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNet.Identity;

namespace Findparts.Controllers
{
    public class WeavyController : Controller
    {
        private readonly IWeavyService _weavyService;        
        public WeavyController(IWeavyService chatService)
        {
            _weavyService = chatService;
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetToken()
        {
            var userId = User.Identity.GetUserId();
            var email = User.Identity.GetEmail();
            string token = _weavyService.GetWeavyToken(userId, email);
           

            return Content(token);
        }

        [Authorize(Roles = "Subscriber, Admin")]
        [HttpPost]
        public async Task<ActionResult> GetMemberId(int vendorId)
        {
            int weavyUserId = await _weavyService.GetMemberId(vendorId);

            return Json(new { id = weavyUserId });
        }

        [Authorize(Roles = "Subscriber, Admin")]
        [HttpPost]
        public async Task<ActionResult> GetCollabChannel(int vendorId)
        {
            try
            {
                int spaceId = await _weavyService.GetCollabChannel(vendorId, User.Identity.GetUserId());
                if (spaceId == 0)
                    return Json(new { error = true, errorMessage = "Failed to create channel" });

                return Json(new { error = false, id = spaceId });
            } catch (Exception ex)
            {
                return Json(new { error = true, errorMessage = ex.Message });
            }            
        }

        [Authorize(Roles = "Subscriber, Admin")]
        [HttpPost]
        public async Task<ActionResult> GetServiceRequestChannel(int vendorId)
        {
            try
            {
                int spaceId = await _weavyService.GetServiceRequestChannel(vendorId, User.Identity.GetUserId());
                if (spaceId == 0)
                    return Json(new { error = true, errorMessage = "Failed to create channel" });

                return Json(new { error = false, id = spaceId });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, errorMessage = ex.Message });
            }
        }
    }
}