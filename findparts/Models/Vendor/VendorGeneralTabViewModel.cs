using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{
    public class VendorGeneralTabViewModel
    {
        public string VendorId { get; set; }
        [Display(Name = "Repair Station Name")]
        public string VendorName { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }
        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; }
        public string DefaultCurrency { get; set; }
    }    
}