using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Vendor
{
    public class QuotePageViewModel
    {
        public QuotePageViewModel()
        {
            Quotes = new List<VendorQuoteGetByVendorID6_Result>();
            AchievementsSelectList = new List<SelectListItem>();
        }
        public List<VendorQuoteGetByVendorID6_Result> Quotes { get; set; }
        public string DefaultCurrency { get; set; }   
        public List<SelectListItem> AchievementsSelectList { get; set; }
        public string VendorID { get; set; }
    }
}