using DAL;
using Findparts.Core;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        [Route("parts")]
        public ActionResult Results(PartsSearchQueryParams queryParams)
        {
            if (queryParams == null) return RedirectToAction("Index");

            if (!string.IsNullOrEmpty(queryParams.PartNumberDetail))
            {
                var result = _partsSearchService.GetDetails(queryParams, User.IsInRole("Admin"));
                return Json(result, JsonRequestBehavior.AllowGet);
            } else if (!string.IsNullOrEmpty(queryParams.Search))
            {
                ViewBag.Title = "Search " + queryParams.Search;
                ViewBag.ExtraTitle = " | Test, Repair & Overhaul capabilities";

                PartsPageViewModel viewModel = new PartsPageViewModel { Text = queryParams.Search };
                string cleanText = new Regex("[^a-zA-Z0-9]").Replace(viewModel.Text, "");
                if (cleanText.Length < Constants.MIN_SEARCH_LENGTH)
                {
                    ViewBag.ErrorMessage = $"Please enter at least {Constants.MIN_SEARCH_LENGTH} alpha-numeric characters";

                } else
                {
                    _partsSearchService.PopulatePartsPageViewModel(viewModel, cleanText, false);
                    return View("~/Views/Home/Parts.cshtml", viewModel);
                }
            } else if (!string.IsNullOrEmpty(queryParams.PartNumber))
            {
                ViewBag.Title = queryParams.PartNumber;
                ViewBag.ExtraTitle = " | search aircraft component MROs - Part 145s";
                ViewBag.ExtraMetaKeywords = "," + Request.QueryString["PartNumber"];

                PartsPageViewModel viewModel = new PartsPageViewModel { Text = queryParams.PartNumber };
                string cleanText = new Regex("[^a-zA-Z0-9]").Replace(viewModel.Text, "");
                if (cleanText.Length < Constants.MIN_SEARCH_LENGTH)
                {
                    ViewBag.ErrorMessage = $"Please enter at least {Constants.MIN_SEARCH_LENGTH} alpha-numeric characters";

                }
                else
                {
                    _partsSearchService.PopulatePartsPageViewModel(viewModel, cleanText, true);
                    return View("~/Views/Home/Parts.cshtml", viewModel);
                }

            }
            else if (string.IsNullOrEmpty(queryParams.Term)) // auto complete
            {
                var result = _partsSearchService.GetPartAutoCompletes(queryParams.Term);
                return Json(result, JsonRequestBehavior.AllowGet);
            } else if (!string.IsNullOrEmpty(queryParams.PreferVendor) && !string.IsNullOrEmpty(queryParams.State))
            {

            } else if (!string.IsNullOrEmpty(queryParams.BlockVendor))
            {

            } else if (!string.IsNullOrEmpty(queryParams.VendorListItemID) && !string.IsNullOrEmpty(queryParams.VendorID))
            {

            } else if (!string.IsNullOrEmpty(queryParams.VendorID) && !string.IsNullOrEmpty(queryParams.VendorListItemID))
            {

            } else if (!string.IsNullOrEmpty(queryParams.DisabledFeatureEmail))
            {

            }

            return RedirectToAction("Index");
        }
    }
}