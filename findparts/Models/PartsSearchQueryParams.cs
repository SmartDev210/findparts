using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models
{
    public class PartsSearchQueryParams
    {
        public string PartNumberDetail { get; set; }
        public string VendorCerts { get; set; }
        public string Search { get; set; }
        public string PartNumber { get; set; }
        public string Term { get; set; }
        public string PreferVendor { get; set; }
        public string BlockVendor { get; set; }
        public string VendorListItemID { get; set; }
        public string DisabledFeatureEmail { get; set; }
        public string VendorID { get; set; }
        public string State { get; set; }
    }
}