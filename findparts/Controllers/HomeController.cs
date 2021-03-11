using DAL;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPartsSearchService _partsSearchService;
       
        public HomeController(IPartsSearchService service)
        {
            _partsSearchService = service;
        }
        public ActionResult Index()
        {
            ViewBag.BodyClass = "home-page";
            ViewBag.ExtraTitle = " | search Test, Repair & OH capabilities for aircraft parts";

            HomePageViewModel viewModel = new HomePageViewModel();
            _partsSearchService.PopulateHomePageViewModel(viewModel);
            return View(viewModel);
        }
    }
}