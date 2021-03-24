using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Subscriber
{
    public class SubscriberAddressPageViewModel
    {
        public SubscriberAddressPageViewModel()
        {
            CountryList = new List<SelectListItem>();
        }
        [Display(Name = "Address1")]
        public string Address1 { get; set; }
        [Display(Name = "Address2")]
        public string Address2 { get; set; }
        [Display(Name = "Address3")]
        public string Address3 { get; set; }
        [Display(Name = "City (Suburb)")]
        public string City { get; set; }
        [Display(Name = "State (Province)")]
        public string State { get; set; }
        [Display(Name = "Zipcode")]
        public string Zipcode { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }
        public List<SelectListItem> CountryList { get; set; }
        [Display(Name = "Main Phone #")]
        public string Phone { get; set; }
    }
}