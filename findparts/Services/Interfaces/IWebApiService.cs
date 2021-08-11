using Findparts.Models;
using Findparts.Models.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Interfaces
{
    public interface IWebApiService
    {
        bool UpdateVendorFromWeavy(Models.ApplicationUser user, UpdateVendorFromWeavyRequest request);
        bool AddVendorUser(ApplicationUser user, ApplicationUser added);
    }
}