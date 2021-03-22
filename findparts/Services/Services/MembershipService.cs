using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Services.Interfaces;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Services.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly FindPartsEntities _context;
        private readonly IMailService _mailService;
        private readonly ApplicationUserManager _userManager;

        public MembershipService(FindPartsEntities context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
            _userManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>();
        }

        public void ApproveUser(ApplicationUser user, bool primaryUser)
        {
            var userProfiles = _context.UserGetByProviderUserKey2(new Guid(user.Id)).ToList();
            if (userProfiles.Count > 0)
            {
                int? vendorID = userProfiles[0].VendorID;
                int? subscriberID = userProfiles[0].SubscriberID;
                string name = userProfiles[0].Name;
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

                SessionVariables.Populate(user.Email);
            }
        }

        public async Task<int> DeleteUser(string userId)
        {
            var user = _context.UserGetByID(userId.ToNullableInt()).FirstOrDefault();
            if (user != null)
            {
                var appUser = await _userManager.FindByEmailAsync(user.Email);
                await _userManager.DeleteAsync(appUser);

                return _context.UserDelete(user.UserID);
            }
            return 0;
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

            UpdateUser(0, new Guid(user.Id), subscriberID.ToNullableInt(), vendorId.ToNullableInt(), user.Email, null);

            return vendorId.HasValue ? vendorId.ToNullableInt().ToString() : null;
        }

        public User GetUserById(string userId)
        {
            return _context.UserGetByID(userId.ToNullableInt()).FirstOrDefault();
        }

        public void UpdateUser(Nullable<int> userID, Nullable<System.Guid> providerUserKey, Nullable<int> subscriberID, Nullable<int> vendorID, string email, Nullable<int> createdByUserID)
        {
            _context.UserUpdate3(userID, providerUserKey, subscriberID, vendorID, email, createdByUserID);
        }
    }
}