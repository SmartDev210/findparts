using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Admin
{
    public class VendorViewModel
    {
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public string DateActivated { get; set; }
        public int UserID { get; set; }
        public int ProfileFieldsCompleted { get; set; }
        public bool MuteStatus { get; set; }
        public bool OEM { get; set; }
        public Nullable<int> PreferredCount { get; set; }
        public Nullable<int> BlockedCount { get; set; }
        public string RecentListApprovalDate { get; set; }
        public Nullable<int> RFQReceivedCount { get; set; }
        public Nullable<int> QuotesSentCount { get; set; }
        public int PartsCount { get; set; }
        public int AchievementsCount { get; set; }
        public int ListsApprovalNeeded { get; set; }
        public int ListsApproved { get; set; }
        public int AchievementsApprovalNeeded { get; set; }
        public int AchievementsApproved { get; set; }
        public Nullable<int> WeavyCompanyId { get; set; }
    }

    public class VendorPagedListViewModel
    {
        public VendorPagedListViewModel()
        {
            Vendors = new List<VendorViewModel>();
        }        
        public int Start { get; set; }
        public int Length { get; set; }        
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<VendorViewModel> Vendors { get; set; }
        public int Draw { get; set; }
    }
}