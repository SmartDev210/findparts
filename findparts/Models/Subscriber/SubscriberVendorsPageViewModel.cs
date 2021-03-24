using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Subscriber
{
    public enum VendorsPageMode
    {
        Preferred = 0,
        Blocked = 1
    }
    public class VendorInfo
    {
        public int VendorID { get; set; }
        public DateTime DateCreated { get; set; }
        public string VendorName { get; set; }
        public string Country { get; set; }
    }
    public class SubscriberVendorsPageViewModel
    {
        public SubscriberVendorsPageViewModel()
        {
            Vendors = new List<VendorInfo>();
        }
        public VendorsPageMode VendorsPageMode { get; set; }
        public List<VendorInfo> Vendors { get; set; }

    }
}