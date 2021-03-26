using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Vendor
{  
    public class CertsViewModel
    {
        public CertsViewModel()
        {
            CertList = new List<VendorCert>();
            CertSelectList = new List<SelectListItem>();
        }
        public string VendorId { get; set; }
        public List<VendorCert> CertList { get; set; }
        public List<SelectListItem> CertSelectList { get; set; }

        [Required]
        public string Cert { get; set; }
        [Required]
        public string Number { get; set; }
    }
}