using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{   
    public class RFQPreferencesViewModel
    {
        public RFQPreferencesViewModel()
        {
            
        }

        public string VendorId { get; set; }
        public string RFQPhone { get; set; }
        public string RFQEmail { get; set; }
        public string RFQWebEmails { get; set; }
        public string RFQFax { get; set; }
    }
}