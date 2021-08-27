using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{
    public class VendorAdvertiseViewModel
    {
        public VendorAdvertiseViewModel()
        {
            VendorPurchases = new List<VendorPurchase>();
        }
        public List<VendorPurchase> VendorPurchases { get; set; }
    }
}