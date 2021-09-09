using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Vendor
{
    public class UploadVendorFileViewModel
    {
        public UploadVendorFileViewModel()
        {
            VendorAchievementTypeSelectList = new List<SelectListItem>();
        }
        public int Id { get; set; }
        public bool IsFirst { get; set; }
        public string Comment { get; set; }
        [Required]
        public int VendorId { get; set; }        
        public HttpPostedFileBase Upload { get; set; }
        public bool ReplaceList { get; set; }
        public bool Approved { get; set; }
        public int VendorAchievementTypeId { get; set; }

        public List<SelectListItem> VendorAchievementTypeSelectList { get; set; }
    }
    public class VendorFileViewModel
    {
        public int VendorId { get; set; }
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string FileType { get; set; }
        public bool ReplaceList { get; set; }
        public DateTime? DateApproved { get; set; }
        public string Comments { get; set; }
        public string AchievementType { get; set; }
    }
    public class VendorUploadListViewModel
    {
        public VendorUploadListViewModel()
        {
            VendorCapabilityList = new List<VendorFileViewModel>();
            // VendorAchievementList = new List<VendorFileViewModel>();
        }
        public List<VendorFileViewModel> VendorCapabilityList { get; set; }
        // public List<VendorFileViewModel> VendorAchievementList { get; set; }
    }
}