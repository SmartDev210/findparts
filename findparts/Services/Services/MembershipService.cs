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

        public async Task<int> DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            user.SubscriberID = null;
            user.VendorID = null;
            user.CreatedByUserID = null;

            return await _context.SaveChangesAsync();
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

        public bool SubscribeWithStripe(int subscriberTypeId, string stripeToken, Subscriber subscriber)
        {
            var plan = _context.SubscriberTypeGetByID(subscriberTypeId).FirstOrDefault();
            
            var user = _context.UserGetFirstBySubscriberID(subscriber.SubscriberID).FirstOrDefault();

            if (plan != null && user != null)
            {
                var stripePlanId = plan.StripePlanID;
                var primaryEmail = user.Email;

                // create customer
                var myCustomer = new Stripe.CustomerCreateOptions
                {
                    Email = primaryEmail,
                    Description = string.Format("{0} ({1})", subscriber.SubscriberName, primaryEmail),
                    Source = stripeToken,
                    Plan = stripePlanId
                };
                var stripeCustomerService = new Stripe.CustomerService();
                var stripeCustomer = stripeCustomerService.Create(myCustomer);

                _context.SubscriberUpdateStripe(subscriber.SubscriberID, stripeCustomer.Id, subscriberTypeId);

                if (stripePlanId == "Starter")
                {
                    _mailService.SendFreeTrialActivated(primaryEmail, subscriber.SubscriberName);
                }
                return true;
            }

            return false;
        }

        public bool UpdateSubscribeWithStripe(string stripeToken, Subscriber subscriber)
        {
            var myCustomer = new Stripe.CustomerUpdateOptions
            {
                Source = stripeToken
            };
            var stripeCustomerService = new Stripe.CustomerService();
            var stripeCustomer = stripeCustomerService.Update(subscriber.StripeCustomerID, myCustomer);
            return true;
        }

        public bool UpdateSubscriptionPlan(Subscriber subscriber, int subscriberTypeId)
        {
            if (string.IsNullOrEmpty(subscriber.StripeCustomerID))
                return false;

            var plan = _context.SubscriberTypeGetByID(subscriberTypeId).FirstOrDefault();
            if (plan == null)
                return false;

            var stripeCustomerService = new Stripe.CustomerService();
            var stripeCustomer = stripeCustomerService.Get(subscriber.StripeCustomerID, new Stripe.CustomerGetOptions
            {
                Expand = new List<string>() { "subscriptions" }
            });


            if (stripeCustomer.Subscriptions.Data.Count == 0)
            {
                // cancelled then changed back to an active plan??
                _context.SubscriberUpdatePendingSubscriberType(subscriber.SubscriberID, subscriberTypeId);
                var subscriptionOptions = new Stripe.SubscriptionCreateOptions
                {
                    TrialPeriodDays = null,

                    Items = new List<Stripe.SubscriptionItemOptions>
                    {
                        new Stripe.SubscriptionItemOptions
                        {
                            Plan = plan.StripePlanID
                        }
                    },
                    Customer = subscriber.StripeCustomerID
                };
                var subscriptionService = new Stripe.SubscriptionService();
                var subscription = subscriptionService.Create(subscriptionOptions);
                return true;
            }
            else
            {
                var stripeSubscription = stripeCustomer.Subscriptions.Data[0];
                var stripeSubscriptionId = stripeSubscription.Id;
                // change plan

                // determine if upgrade or downgrade

                bool upgrade = false;
                bool upgradeRequiresManualInvoiceAndPay = false;

                var existingType = _context.SubscriberTypeGetByID(subscriber.SubscriberTypeID).FirstOrDefault();
                if (existingType != null)
                {
                    bool existingAnnual = existingType.Annual;
                    bool pendingAnnual = plan.Annual;
                    int existingLevel = existingType.Level;
                    int pendingLevel = plan.Level;
                    if (existingAnnual == pendingAnnual)
                    {
                        upgrade = existingLevel < pendingLevel;
                        upgradeRequiresManualInvoiceAndPay = true;
                    }
                    else if (existingAnnual)
                    {
                        // annual to monthly
                        upgrade = existingLevel < pendingLevel;
                        upgradeRequiresManualInvoiceAndPay = false;
                    }
                    else
                    {
                        // month to annual
                        upgrade = existingLevel <= pendingLevel;
                        upgradeRequiresManualInvoiceAndPay = false;
                    }
                }
                if (upgrade)
                {
                    // set pending subscriber type
                    _context.SubscriberUpdatePendingSubscriberType(subscriber.SubscriberID, subscriberTypeId);

                    var newSubscription = new Stripe.SubscriptionUpdateOptions
                    {
                        Items = new List<Stripe.SubscriptionItemOptions> { new Stripe.SubscriptionItemOptions { Id = stripeSubscription.Items.Data[0].Id,  Plan = plan.StripePlanID, } }
                    };
                    var subscriptionService = new Stripe.SubscriptionService();
                    var newStripeSubscription = subscriptionService.Update(stripeSubscriptionId, newSubscription);
                    

                    if (upgradeRequiresManualInvoiceAndPay)
                    {
                        //create invoice
                        var invoiceService = new Stripe.InvoiceService();
                        var stripeInvoice = invoiceService.Create(new Stripe.InvoiceCreateOptions { Customer = subscriber.StripeCustomerID });

                        //pay invoice
                        stripeInvoice = invoiceService.Pay(stripeInvoice.Id);
                        if (!stripeInvoice.Paid)
                        {
                            // TODO: alert user/admin of failed instant attempt? maybe not, since fails in users face

                            // TODO: might not be needed. stripe might revert back on its own. possible duplication here??

                            // change plan back. this is so users cant upgrade without paying immediately
                            newSubscription.Items[0].Plan = existingType.StripePlanID;
                            stripeSubscription = subscriptionService.Update(stripeSubscriptionId, newSubscription);

                            // close invoice so stripe does not retry charging later
                            var stripeInvoiceUpdateOptions = new Stripe.InvoiceUpdateOptions()
                            {
                                AutoAdvance = false
                            };

                            stripeInvoice = invoiceService.Update(stripeInvoice.Id, stripeInvoiceUpdateOptions);

                            return false;
                        }
                        else
                        {
                            // send email
                            var user = _context.UserGetFirstBySubscriberID(subscriber.SubscriberID).FirstOrDefault();
                            if (user != null)
                            {
                                _mailService.SendUpDowngradeEmail(subscriber.SubscriberName, user.Email, upgrade);
                            }
                            _mailService.SendAdminUpDowngradeEmail(subscriber.SubscriberName, upgrade);


                            // TODO: might not be needed, webhook might take care of this
                            _context.SubscriberUpdateSubscriberType(subscriber.SubscriberID, subscriberTypeId);
                            _context.SubscriberUpdatePendingSubscriberType(subscriber.SubscriberID, null);

                            return true;
                        }
                    } else
                    {
                        // send emails
                        var user = _context.UserGetFirstBySubscriberID(subscriber.SubscriberID).FirstOrDefault();
                        if (user != null)
                        {
                            _mailService.SendUpDowngradeEmail(subscriber.SubscriberName, user.Email, upgrade);
                        }
                        _mailService.SendAdminUpDowngradeEmail(subscriber.SubscriberName, upgrade);

                        //webhook changes subscriberTypeID

                        return true;
                    }
                } else
                {
                    // set pending subscriber type
                    _context.SubscriberUpdatePendingSubscriberType(subscriber.SubscriberID, subscriberTypeId);

                    // cancel with stripe at period end
                    var subscriptionService = new Stripe.SubscriptionService();
                    subscriptionService.Update(stripeSubscriptionId, new Stripe.SubscriptionUpdateOptions { CancelAtPeriodEnd = true } );

                    // send emails
                    var user = _context.UserGetFirstBySubscriberID(subscriber.SubscriberID).FirstOrDefault();
                    if (user != null)
                    {
                        _mailService.SendUpDowngradeEmail(subscriber.SubscriberName, user.Email, upgrade);
                    }
                    _mailService.SendAdminUpDowngradeEmail(subscriber.SubscriberName, upgrade);

                    // wait for webhooks to do the rest
                    return true;
                }
            }
        }

        public bool CancelSubscription(Subscriber subscriber, string stripeSubscriptionId)
        {
            // set pending cancellation
            _context.SubscriberUpdatePendingSubscriberType(subscriber.SubscriberID, (int)SubscriberTypeID.NoCreditCard);

            // cancel with stripe at period end

            var options = new Stripe.SubscriptionUpdateOptions();
            options.CancelAtPeriodEnd = true;            

            var subscriptionService = new Stripe.SubscriptionService();
            subscriptionService.Update(stripeSubscriptionId, options);


            // send emails
            var user = _context.UserGetFirstBySubscriberID(subscriber.SubscriberID).FirstOrDefault();
            if (user != null)
            {   
                _mailService.SendSubscriberCancelledEmail(user.Email, subscriber.SubscriberName);
            }
            _mailService.SendAdminSubscriptionCancelledEmail(subscriber.SubscriberName);

            // wait for webhooks to do the rest
            return true;
        }

        public void PopulateRegisterViewModel(ExternalLoginConfirmationViewModel viewModel)
        {
            viewModel.CountryList = Constants.Countries.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();
        }
        private bool Charge(int vendorId, string stripeToken, string email, PurchaseType type, int quantity, int moneyPer1000Views) // moneyPer1000Views
        {
            var chargeOptions = new Stripe.ChargeCreateOptions
            {
                Amount = moneyPer1000Views * quantity / 1000 * 100, 
                Description = $"Purchase for {type.GetDisplayName()} (vendor:{vendorId} - email:{email})",
                Source = stripeToken,
                Currency = "usd",
                ReceiptEmail = email,
            };
            var chargeService = new Stripe.ChargeService();
            var charge = chargeService.Create(chargeOptions);

            _context.VendorPurchases.Add(new VendorPurchase
            {
                VendorId = vendorId,
                Amount = charge.Amount,
                PurchasedAt = DateTime.UtcNow,
                PurchaseType = (int)type,
                Quantity = quantity,
                StripeChargeId = charge.Id
            });
            return _context.SaveChanges() > 0;
        }
        public bool PurchaseWithStripe(string vendorID, string stripeToken)
        {
            var user = _context.UserGetByVendorID(vendorID.ToNullableInt()).FirstOrDefault();            

            if (user != null)
            {
                return Charge(vendorID.ToInt(), stripeToken, user.Email, PurchaseType.Advertise, 1, 150000);
            }

            return false;
        }
        public bool PurchaseImpressionsWithStripe(string vendorID, string stripeToken, int quantity, PurchaseType type)
        {
            var user = _context.UserGetByVendorID(vendorID.ToNullableInt()).FirstOrDefault();

            if (user != null)
            {
                int moneyPer1000Views = 50;
                if (type == PurchaseType.OrganicAllImpressions)
                {
                    moneyPer1000Views = 20;
                } else if (type == PurchaseType.OrganicTargetImpressions)
                {
                    moneyPer1000Views = 100;
                } else if (type == PurchaseType.SponsoredTargetImpressions)
                {
                    moneyPer1000Views = 50;
                }
                
                return Charge(vendorID.ToInt(), stripeToken, user.Email, type, quantity, moneyPer1000Views);
            }

            return false;
        }
    }
}