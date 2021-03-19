using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models.Subscriber;
using Findparts.Services.Interfaces;

namespace Findparts.Controllers
{
    public class SubscriberController : Controller
    {
        private readonly ISubscriberService _service;

        public SubscriberController(ISubscriberService service)
        {
            _service = service;
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
    }
}