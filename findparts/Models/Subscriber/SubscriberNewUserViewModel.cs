using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Findparts.Models.Subscriber
{
    public class SubscriberNewUserViewModel
    {   
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        public int? VendorID { get; set; }
        public bool VendorAdmin { get; set; }
    }
}