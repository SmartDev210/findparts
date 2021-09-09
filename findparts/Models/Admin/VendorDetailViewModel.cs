using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Admin
{
    public class VendorDetailViewModel
    {
        public VendorDetailViewModel()
        {
            StatusSelectList = new List<SelectListItem>();
            VendorList = new List<VendorListGetByVendorID_Result>();
            //VendorAchievementList = new List<VendorAchievementListGetByVendorID2_Result>();
            SubscriberId = null;
        }
        public int VendorId { get; set; }
        public int? SubscriberId { get; set; }
        public string VendorName { get; set; }
        public int Status { get; set; }
        public List<SelectListItem> StatusSelectList { get; set; }
        public bool MuteWorkscopeIcons { get; set; }
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }

        public List<VendorListGetByVendorID_Result> VendorList { get; set; }
        //public List<VendorAchievementListGetByVendorID2_Result> VendorAchievementList { get; set; }
    }
}