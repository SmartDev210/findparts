using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{
    public class OEMsViewModel
    {
        public string VendorId { get; set; }
        public bool IsOEM { get; set; }
        public bool OEMExclusive { get; set; }
        public bool OEMRequiresRMA { get; set; }
    }
}