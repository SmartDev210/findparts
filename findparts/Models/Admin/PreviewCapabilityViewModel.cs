using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Admin
{
    public class PreviewCapabilityViewModel
    {
        public PreviewCapabilityViewModel()
        {
            Items = new List<VendorListItem>();
        }
        public int VendorListId { get; set; }
        public bool Preview { get; set; }
        public List<VendorListItem> Items { get; set; }
    }
    public class PreviewAchievementViewModel
    {
        public PreviewAchievementViewModel()
        {
            Items = new List<VendorAchievementListItem>();
        }
        public int VendorAchievementListId { get; set; }
        public bool Preview { get; set; }
        public List<VendorAchievementListItem> Items { get; set; }
    }
}