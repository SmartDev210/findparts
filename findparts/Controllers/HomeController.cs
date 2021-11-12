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
                var result = _partsSearchService.GetDetails(queryParams, User.IsInRole("Admin"), User.Identity.IsAuthenticated);
                return Json(result, JsonRequestBehavior.AllowGet);
            } else if (!string.IsNullOrEmpty(queryParams.Search))
            {   
                if (Config.PortalCode == 0)
                {
                    ViewBag.Title = "Search " + queryParams.Search + " - buddy.aero Repair Stations Overhaul MRO";
                }
                else
                {
                    ViewBag.Title = "Search " + queryParams.Search + " – buddy.aero Aircraft Spare Parts Aviation";
                }

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
                if (Config.PortalCode == 0)
                {
                    ViewBag.Title = queryParams.PartNumber + " - buddy.aero Repair Stations Overhaul MRO";
                }
                else
                {
                    ViewBag.Title = queryParams.PartNumber + " – buddy.aero Aircraft Spare Parts Aviation";
                }
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
            else if (!string.IsNullOrEmpty(queryParams.Term)) // auto complete
            {
                var result = _partsSearchService.GetPartAutoCompletes(queryParams.Term);
                return Json(result, JsonRequestBehavior.AllowGet);
            } else if (!string.IsNullOrEmpty(queryParams.PreferVendor) && !string.IsNullOrEmpty(queryParams.State))
            {
                _partsSearchService.PreferBlockVendor(queryParams.PreferVendor, true, queryParams.State);
                return new EmptyResult();
            } else if (!string.IsNullOrEmpty(queryParams.BlockVendor))
            {
                _partsSearchService.PreferBlockVendor(queryParams.BlockVendor, false, queryParams.State);

            } else if (!string.IsNullOrEmpty(queryParams.VendorListItemID) && !string.IsNullOrEmpty(queryParams.VendorID))
            {
                _partsSearchService.SendRFQ(queryParams.VendorID, queryParams.VendorListItemID, queryParams.Comments, queryParams.RFQID);
            }  else if (!string.IsNullOrEmpty(queryParams.DisabledFeatureEmail))
            {
                _partsSearchService.SendDisabledFeatureEmail(queryParams.DisabledFeatureEmail);
            }

            return RedirectToAction("Index");
        }
    }
}