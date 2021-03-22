using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Subscriber
{
    public enum SubscriberActiveTab
    {
        GeneralTab = 0,
        UsersTab = 1,
        InvoicesTab = 2,
        AddressTab = 3,
        FavoritesTab = 4,
        BlockedMROsTab = 5
    }
    public class SubscriberIndexPageViewModel
    {
        public SubscriberIndexPageViewModel() { }
        public string SubscriberName { get; set; }
        public string SubscriberType { get; set; }
        public int UserCount { get; set; }
        public int UserQuota { get; set; }
        public int SearchCount { get; set; }
        public int SearchQuota { get; set; }
        public string Country { get; set; }
    }
}