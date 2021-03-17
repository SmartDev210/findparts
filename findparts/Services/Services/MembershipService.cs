using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public void ApproveUser(ApplicationUser user, bool primaryUser)
        {
            var userProfile = _context.UserGetByProviderUserKey2(new Guid(user.Id)).FirstOrDefault();
            if (userProfile != null)
            {
                int? vendorID = userProfile.VendorID;
                int? subscriberID = userProfile.SubscriberID;
                string name = userProfile.Name;
                bool vendor = vendorID.HasValue;
                _context.UserUpdateDateActivated(new Guid(user.Id));

                if (primaryUser)
                {
                    _mailService.SendAccountActivated(user.Email, name, vendor);
                    _mailService.SendAdminAccountActivatedEmail(name, vendor);
                }
                else
                {
                    // TODO: send different version 
                    _mailService.SendAccountActivated(user.Email, name, vendor);
                    _mailService.SendAdminAccountActivatedEmail(name, vendor);
                }

                SessionVariables.Populate(user.UserName);
            }
        }

        public Subscriber GetSubscriberById(string subscriberId)
        {
            return _context.SubscriberGetByID(subscriberId.ToNullableInt()).FirstOrDefault();
        }

        public void PopulateRegisterViewModel(RegisterViewModel viewmModel)
        {
            viewmModel.CountryList = Constants.Countries.Select(x => new SelectListItem {
                Value = x,
                Text = x
            }).ToList();
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