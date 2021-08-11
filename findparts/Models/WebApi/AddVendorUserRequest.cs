using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.WebApi
{
    public class AddVendorUserRequest
    {
        public string CreatorEmail { get; set; }
        public string Email { get; set; }
    }
}