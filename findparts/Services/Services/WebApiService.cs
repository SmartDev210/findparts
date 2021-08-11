using DAL;
using Findparts.Models;
using Findparts.Models.WebApi;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class WebApiService : IWebApiService
    {
        private readonly FindPartsEntities _context;
        public WebApiService(FindPartsEntities context)
        {
            _context = context;
        }

        public bool AddVendorUser(ApplicationUser creator, ApplicationUser added)
        {
            var userProfile = _context.Users.FirstOrDefault(x => x.ProviderUserKey == new Guid(creator.Id));
            var addedProfile = _context.Users.FirstOrDefault(x => x.ProviderUserKey == new Guid(added.Id));

            if (userProfile == null || addedProfile == null)
                return false;

            addedProfile.CreatedByUserID = userProfile.UserID;
            addedProfile.VendorID = userProfile.VendorID;
            addedProfile.SubscriberID = userProfile.SubscriberID;

            return _context.SaveChanges() > 0;
        }

        public bool UpdateVendorFromWeavy(Models.ApplicationUser user, UpdateVendorFromWeavyRequest request)
        {
            var userProfile = _context.Users.FirstOrDefault(x => x.ProviderUserKey == new Guid(user.Id));
            if (userProfile == null) return false;

            if ((userProfile.VendorID ?? 0) == 0)
            {
                var vendor = _context.Vendors.Add(new Vendor
                {
                    VendorName = request.Name,
                    RFQEmail = request.Email,
                    Address1 = request.Location,
                    WebsiteURL = request.Website,
                    Phone = request.Phone,
                    Notes = request.Description,
                    DateCreated = DateTime.UtcNow,
                });
                _context.SaveChanges();
                userProfile.VendorID = vendor.VendorID;
            } else
            {   
                var vendor = _context.Vendors.FirstOrDefault(x => x.VendorID == userProfile.VendorID.Value);
                vendor.VendorName = request.Name;
                vendor.RFQEmail = request.Email;
                vendor.Phone = request.Phone;
                vendor.WebsiteURL = request.Website;
                vendor.Notes = request.Description;
                vendor.Address1 = request.Location;
                vendor.WeavyCompanyId = request.CompanyId;
            }
            
            return _context.SaveChanges() > 0;
        }
    }
}