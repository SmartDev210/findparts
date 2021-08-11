using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.WebApi
{
    public class UpdateVendorFromWeavyRequest
    {
        public string UserEmail { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Certs { get; set; }
        public int CompanyId { get; set; }
    }
}