using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{
    public enum VendorActiveTab
    {
        GeneralTab = 0,
        RFQTab = 1,
        CertsTab = 2,
        AddressTab = 3,
        OEMsOnlyTab = 4,
        BlockedSubscribersTab = 5,
        InvoicesTab = 6,
        UsersTab = 7,
        AdvertiseTab = 8,
    }
    
    public class VendorIndexPageViewModel
    {   
        public string VendorId { get; set; }
        public VendorGeneralTabViewModel VendorGeneralTabViewModel { get; set; }
        public RFQPreferencesViewModel RFQPreferencesViewModel { get; set; }
        public AddressViewModel AddressViewModel { get; set; }
        public OEMsViewModel OEMsViewModel { get; set; }
        public CertsViewModel CertsViewModel { get; set; }
    }
}