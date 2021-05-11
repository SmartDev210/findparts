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
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [Authorize]
        [HttpPost]
        public ActionResult GetToken()
        {
            var userId = User.Identity.GetUserId();
            var email = User.Identity.GetEmail();
            string token = _chatService.GetWeavyToken(userId, email);
           

            return Content(token);
        }

        [Authorize(Roles = "Subscriber, Admin")]
        [HttpPost]
        public async Task<ActionResult> GetMemberId(int vendorId)
        {
            int weavyUserId = await _chatService.GetMemberId(vendorId);

            return Json(new { id = weavyUserId });
        }
    }
}