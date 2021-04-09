using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Admin
{
    public class SubscriberViewModel
    {
        public int UserID { get; set; }
        public int SubscriberID { get; set; }
        public string SubscriberName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public string DateActivated { get; set; }
        public int ProfileFieldsCompleted { get; set; }
        public string MembershipLevel { get; set; }
        public int SearchCount { get; set; }
        public int InvoiceCount { get; set; }
        public int UserCount { get; set; }
        public string RecentInvoiceDate { get; set; }
        public int RFQSentCount { get; set; }
        public int QuotesReceivedCount { get; set; }
        public int PreferredCount { get; set; }
        public int BlockedCount { get; set; }
        public int BlockedByVendorCount { get; set; }
        public bool MRO { get; set; }
        public int EmailDomains { get; set; }
    }

    public class SubscriberPagedListViewModel
    {
        public SubscriberPagedListViewModel()
        {
            Subscribers = new List<SubscriberViewModel>();
        }
        public int Start { get; set; }
        public int Length { get; set; }
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<SubscriberViewModel> Subscribers { get; set; }
        public int Draw { get; set; }
    }
}