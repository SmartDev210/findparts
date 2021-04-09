using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Admin
{
    public class SubscriberDetailViewModel
    {
        public SubscriberDetailViewModel()
        {
            StatusSelectList = new List<SelectListItem>();
        }
        public int? VendorId { get; set; }
        [Required]
        public int SubscriberId { get; set; }
        [Required]
        public string SubscriberName { get; set; }
        public int StatusId { get; set; }
        public int? SignupSubscriberTypeId { get; set; }
        public string SignupSubscriberTypeText { get; set; }
        public List<SelectListItem> StatusSelectList { get; set; }
        public string Notes { get; set; }
    }
}