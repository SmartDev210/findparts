using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using NLog;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Controllers
{
    public class StripeWebhooksController : Controller
    {
		private readonly FindPartsEntities _context;
		private readonly IMailService _mailService;
		private readonly ISubscriberService _subscriberService;
		public StripeWebhooksController(FindPartsEntities entities, IMailService mailService, ISubscriberService subscriberService)
        {
			_context = entities;
			_mailService = mailService;
			_subscriberService = subscriberService;
        }
        // GET: StripeWebhooks
		[AllowAnonymous]
        public ActionResult Index()
        {
            if (Request.QueryString["secret"] == Config.StripeSecret)
            {
                var json = new StreamReader(Request.InputStream).ReadToEnd();
                Event stripeEvent = EventUtility.ParseEvent(json);
                string eventId;
                
                if (stripeEvent == null)
                {
                    return new EmptyResult();
                }
                eventId = stripeEvent.Id;

				// dont trust data in webhook. get data via eventID
				/*
                var eventService = new StripeEventService();
                if (!new Regex("^[a-z]+_0+$").IsMatch(stripeEvent.Id))
                {
                    stripeEvent = eventService.Get(eventId);
                }
                 */

				// TODO: add logic to prevent multiple deliveries of the same webhook from causing problems
				string errorText = "";
				try
				{
					switch (stripeEvent.Type)
					{
						case Events.Ping:
							break;
						//case StripeEvents.CustomerSubscriptionCreated:
						// subscription created on existing customer. canceled and resigned up
						// break;
						case Events.InvoiceCreated:
							{
								// do nothing here. wait until InvoicePaymentSucceeded/InvoicePaymentFailed
								//StripeInvoice stripeInvoice = Stripe.Mapper<StripeInvoice>.MapFromJson(stripeEvent.Data.Object.ToString());
								//CreateInvoice(stripeInvoice);
							}
							break;
						case Events.InvoicePaymentFailed:
							{	
								var stripeInvoice = (Invoice)stripeEvent.Data.Object;
								// ??
								
								var subscriber = _context.SubscriberGetByStripeCustomerID(stripeInvoice.CustomerId).FirstOrDefault();
								if (subscriber != null)
								{
									int subscriberID = subscriber.SubscriberID;
									int subscriberTypeID = subscriber.SubscriberTypeID;
									string subscriberName = subscriber.SubscriberName.ToString();
									int? pendingSubscriberTypeID = subscriber.PendingSubscriberTypeID;

									if (subscriberTypeID != (int)SubscriberTypeID.NoCreditCard)
									{
										_mailService.SendAdminFailedTransactionEmail(subscriberName);
										var user = _context.UserGetFirstBySubscriberID(subscriberID).FirstOrDefault();
										if (user != null)
										{
											string primaryUserEmail = user.Email;
											_mailService.SendFailedTransactionEmail(subscriberName, primaryUserEmail);
										}
										if (pendingSubscriberTypeID != null)
										{
											_context.SubscriberUpdatePendingSubscriberType(subscriberID, subscriberTypeID);
										}
										_context.SubscriberUpdateSubscriberType(subscriberID, (int)SubscriberTypeID.NoCreditCard);
									}
								}
							}
							break;
						case Events.InvoicePaymentSucceeded:
							{
								int subscriberInvoiceID;
								Invoice stripeInvoice = (Invoice)stripeEvent.Data.Object;
								// update invoice.DatePaid
								
								// get invoice
								var subscriber = _context.SubscriberInvoiceGetByStripeInvoiceID(stripeInvoice.Id).FirstOrDefault();
								if (subscriber == null)
								{
									// generate if does not exist
									subscriberInvoiceID = CreateInvoice(stripeInvoice);
								}
								else
								{
									subscriberInvoiceID = subscriber.SubscriberInvoiceID;
								}
								// update paid
								_context.SubscriberInvoiceUpdatePaid(subscriberInvoiceID);

								// update SubscriberTypeID if pending
								var subscriber2 = _context.SubscriberGetByStripeCustomerID(stripeInvoice.CustomerId).FirstOrDefault();
								if (subscriber2 != null)
								{
									int subscriberID = subscriber2.SubscriberID;
									int subscriberTypeID = subscriber2.SubscriberTypeID;
									int? pendingSubscriberTypeID = subscriber2.PendingSubscriberTypeID;
									if (pendingSubscriberTypeID != null && subscriberTypeID != pendingSubscriberTypeID)
									{
										_context.SubscriberUpdateSubscriberType(subscriberID, pendingSubscriberTypeID);
										_context.SubscriberUpdatePendingSubscriberType(subscriberID, null);
									}
								}
							}
							break;
						case Events.CustomerSubscriptionDeleted:
							{
								var stripeSubscription = stripeEvent.Data.Object as Subscription;
								// check if pending cancellation
								if (stripeSubscription.CancelAtPeriodEnd)
								{	
									var subscriber = _context.SubscriberGetByStripeCustomerID(stripeSubscription.CustomerId).FirstOrDefault();
									if (subscriber != null)
									{
										int subscriberID = subscriber.SubscriberID;
										int subscriberTypeID = subscriber.SubscriberTypeID;
										int? pendingSubscriberTypeID = subscriber.PendingSubscriberTypeID;
										if (pendingSubscriberTypeID == (int)SubscriberTypeID.NoCreditCard)
										{
											// update subscriberType and set pending back to null
											_context.SubscriberUpdateSubscriberType(subscriberID, (int)SubscriberTypeID.NoCreditCard);
											_context.SubscriberUpdatePendingSubscriberType(subscriberID, null);
										}
										else if (pendingSubscriberTypeID != null)
										{
											// update subscriberType and set pending back to null
											_context.SubscriberUpdateSubscriberType(subscriberID, (int)SubscriberTypeID.NoCreditCard);

											var subscriberType = _context.SubscriberTypeGetByID(pendingSubscriberTypeID).FirstOrDefault();
											if (subscriberType != null)
											{
												string stripePlanID = subscriberType.StripePlanID;

												// no trial on plan change
												var subscriptionOptions = new SubscriptionCreateOptions()
												{
													Customer = stripeSubscription.CustomerId,
													TrialPeriodDays = null,
													Items = new List<SubscriptionItemOptions>() { new SubscriptionItemOptions { Plan = stripePlanID } }
												};
																								
												var subscriptionService = new SubscriptionService();
												subscriptionService.Create(subscriptionOptions);
											}
										}
									}
								}
							}
							break;
						
						case Events.ChargeFailed:
							{
								var stripeCharge = stripeEvent.Data.Object as Charge;
								var charge = _context.VendorPurchases.Where(x => x.StripeChargeId == stripeCharge.Id).FirstOrDefault();
								if (charge != null)
                                {
									var user = _context.Users.FirstOrDefault(x => x.VendorID == charge.VendorId);
									var vendor = _context.Vendors.Find(charge.VendorId);
									if (user != null)
                                    {
										_mailService.SendStripeChargeFailedEmail(user.Email, vendor.VendorName);
									}
                                }
							}
							break;
						case Events.ChargeSucceeded:
							{
								var stripeCharge = stripeEvent.Data.Object as Charge;
								var charge = _context.VendorPurchases.Where(x => x.StripeChargeId == stripeCharge.Id).FirstOrDefault();
								if (charge != null)
								{
									var user = _context.Users.FirstOrDefault(x => x.VendorID == charge.VendorId);
									var vendor = _context.Vendors.Find(charge.VendorId);
									if (user != null)
									{
										_mailService.SendStripeChargeSucceededEmail(user.Email, vendor.VendorName, stripeCharge.Amount);
									}
								}
							}
							break;
						
						default:
							// send email to rob
							_mailService.SendWebhookEmail(stripeEvent.Type.ToString(), errorText + json);
							break;
					}
				}
				catch (Exception ex)
				{
					errorText = "Error: " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
					// send email to rob
					_mailService.SendWebhookEmail(stripeEvent.Type.ToString(), errorText + json);

					var logger = LogManager.GetLogger("errorLogger");
					logger.Log(LogLevel.Debug, $"==============================================================================================================================");
					logger.Log(LogLevel.Debug, $"");
					logger.Log(LogLevel.Debug, $"Stripe web hook handle exception");
					logger.Log(LogLevel.Debug, $"Date: {DateTime.Now.ToString()}");
					logger.Log(LogLevel.Debug, errorText);
					logger.Log(LogLevel.Debug, json);

					throw ex;
				}
			}
			return new EmptyResult();
        }

		private int CreateInvoice(Invoice stripeInvoice)
		{
			string stripeCustomerID = stripeInvoice.CustomerId;
			string stripeInvoiceID = stripeInvoice.Id;
			long amount = stripeInvoice.AmountDue;
			long startDate = 0;
			long endDate = 0;
			if (stripeInvoice.Lines.Data.Count > 0)
			{
				startDate = stripeInvoice.Lines.Data[0].Period.Start;
				endDate = stripeInvoice.Lines.Data[0].Period.End;
			}

			
			var subscriber = _context.SubscriberGetByStripeCustomerID(stripeCustomerID).FirstOrDefault();
			if (subscriber != null)
			{
				int subscriberID = subscriber.SubscriberID;
				string subscriberName = subscriber.SubscriberName;
				if (subscriberID != 0)
				{
					
					decimal? subscriberInvoiceID = _context.SubscriberInvoiceInsert(subscriberID, stripeInvoiceID, (int)amount, startDate.FromUnixTime(), endDate.FromUnixTime()).FirstOrDefault();

					// generate invoice pdf, save to disk
					_subscriberService.CreateInvoice(stripeInvoiceID, subscriberID.ToString());

					var user = _context.UserGetFirstBySubscriberID(subscriberID).FirstOrDefault();
					if (user != null)
					{
						string primaryUserEmail = user.Email;
						// email to users
						_mailService.SendInvoiceCreatedEmail(subscriberName, primaryUserEmail, (amount / 100m).ToString("c"));
					}

					return subscriberInvoiceID.ToNullableInt() ?? 0;
				}
			}
			return 0;
		}
	}
}