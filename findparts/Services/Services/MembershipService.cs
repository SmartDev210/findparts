using DAL;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly FindPartsEntities _context;
        private readonly FindPartsIdentityEntities _identityContext;
        private readonly IMailService _mailService;

        public MembershipService(FindPartsEntities context, FindPartsIdentityEntities identityContext, IMailService mailService)
        {
            _context = context;
            _identityContext = identityContext;
            _mailService = mailService;
        }

        public Subscriber GetSubscriberById(string subscriberId)
        {
            int id = 0;
            if (Int32.TryParse(subscriberId, out id)) {
                var result = _context.SubscriberGetByID(id).ToList();
                if (result.Count > 0)
                {
                    return result[0];
                }
            }
            return null;
        }

        public string RegisterNewUser(RegisterViewModel model, ApplicationUser user)
        {
            var subscriberID = _context.SubscriberInsert2(model.CompanyName, model.Country, model.PhoneNumber, model.SubscriberTypeId).FirstOrDefault();
            string planSelected = null;
            if (model.SubscriberTypeId.HasValue)
            {
                planSelected = _context.SubscriberTypeGetByID(model.SubscriberTypeId).FirstOrDefault()?.Name;
            }

            decimal? vendorId = null;

            if (model.VendorSignup)
            {
                vendorId = _context.VendorInsert4(model.CompanyName, model.Country, model.PhoneNumber, model.PhoneNumber, user.Email, user.Email).FirstOrDefault();

                _mailService.SendAdminNewAccountEmail(model.CompanyName, true, planSelected);
            }
            else
            {
                _mailService.SendAdminNewAccountEmail(model.CompanyName, false, planSelected);
            }

            _context.UserUpdate3(0, new Guid(user.Id), subscriberID.ToNullableInt(), vendorId.ToNullableInt(), user.Email, null);

            return vendorId.HasValue ? vendorId.ToNullableInt().ToString() : null;
        }
    }
}