using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Findparts.Services.Services
{
    public class MailService : IMailService
    {
		private readonly FindPartsEntities _context;
		public MailService(FindPartsEntities context)
        {
			_context = context;
        }
        private string VendorSubscriber(bool vendor)
        {
            return vendor ? "Vendor" : "Subscriber";
        }

        private string CapabilityMerit(bool capability, int portalCode)
        {
            return capability ? (portalCode == 0 ? "Repair Capability" : "✈ Spare Parts") : "Workscope Icon";
        }

        private string CapabilitiesMerits(bool capability, int portalCode)
        {
			return capability ? (portalCode == 0 ? "Repair Capabilities" : "✈ Spare Parts") : "Workscope Icons";
        }

        private bool SendEmail(string from, string to, string subject, string body, string bcc = null, bool html = false)
        {	
			MailMessage message = new MailMessage(from, to, subject, body);
			
			if (html) message.IsBodyHtml = true;

			if (!string.IsNullOrEmpty(bcc))
			{
				message.Bcc.Add(bcc);
			}
			if (!string.IsNullOrEmpty(Config.BccEmail))
			{
				message.Bcc.Add(Config.BccEmail);
			}

			if (Config.Environment == "live")
            {
				using (SmtpClient client = new SmtpClient(Config.SMTPHost, Config.SMTPPort.ToInt()))
				{
					client.UseDefaultCredentials = false;

					// Create a network credential with your SMTP user name and password.
					client.Credentials = new NetworkCredential(Config.SMTPUsername, Config.SMTPPassword);

					// Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
					// the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
					client.EnableSsl = true;

					client.DeliveryMethod = SmtpDeliveryMethod.Network;


					// Send the email. 
					try
					{	
						client.Send(message);
					}
					catch (Exception ex)
					{
						var debugLogger = LogManager.GetLogger("errorLogger");
						debugLogger.Log(LogLevel.Error, $"==============================================================================================================================");						
						debugLogger.Log(LogLevel.Error, $"Exception while sending email");
						debugLogger.Log(LogLevel.Error, $"{ex.Message}");
						debugLogger.Log(LogLevel.Error, $"{ex.StackTrace}");
						debugLogger.Log(LogLevel.Error, JsonConvert.SerializeObject(ex));
						debugLogger.Log(LogLevel.Error, $"");
						return false;
					}
				}
			} 

			var logger = LogManager.GetLogger("emailLogs");
			logger.Log(LogLevel.Info, $"==============================================================================================================================");
			logger.Log(LogLevel.Info, $"");
			logger.Log(LogLevel.Info, $"Send Email through ses smtp server");
			logger.Log(LogLevel.Info, $"Date sent: {DateTime.Now.ToString()}");
			logger.Log(LogLevel.Info, JsonConvert.SerializeObject(message));
			logger.Log(LogLevel.Info, $"");

            return true;
        }

        #region admin
        public void SendAdminUploadEmail(string vendorName, string vendorID, bool capabilityList)
        {
            string message = "A new " + CapabilityMerit(capabilityList, Config.PortalCode) + " list has been uploaded," + Environment.NewLine
                + Environment.NewLine
                + vendorName + Environment.NewLine
                + Environment.NewLine
                + "Click below to view Vendors:" + Environment.NewLine +
                "https://" + HttpContext.Current.Request.Url.Host + "/Admin/Vendors/" + vendorID + Environment.NewLine;

            SendEmail(Config.FromEmail, Config.AdminEmail, CapabilityMerit(capabilityList, Config.PortalCode) + " List Uploaded" + vendorName, message);
        }


        public void SendAdminNewAccountEmail(string companyName, bool vendor, string planSelected)
        {
            string message = "A new " + VendorSubscriber(vendor) + " has signed up." + Environment.NewLine
            + Environment.NewLine
            + companyName + Environment.NewLine
            + Environment.NewLine
            + (string.IsNullOrEmpty(planSelected) ? "" : "Plan Selected (" + planSelected + ")");

            SendEmail(Config.FromEmail, Config.AdminEmail, "New " + VendorSubscriber(vendor) + " - " + companyName, message, Config.AdminBccEmail);
        }

        public void SendAdminNewUserEmail(string email, string subscriberName, string createdBy)
        {
            string message = "A new user has been created." + Environment.NewLine
                + Environment.NewLine
                + "New User: " + email + Environment.NewLine
                + Environment.NewLine
                + "Account: " + subscriberName + Environment.NewLine
                + Environment.NewLine
                + "Created by: " + createdBy;

            SendEmail(Config.FromEmail, Config.AdminEmail, "New User Created - " + subscriberName, message);
        }

        public void SendAdminAccountActivatedEmail(string name, bool vendor)
        {
            string message = VendorSubscriber(vendor) + " has Activated their Account" + (vendor ? $", Pending {Config.PortalName} Approval" : "") + Environment.NewLine
                + Environment.NewLine
                + name;

            SendEmail(Config.FromEmail, Config.AdminEmail, VendorSubscriber(vendor) + " Activated - " + name, message);
        }

        public void SendAdminDuplicateSignupEmail(string name, bool vendor)
        {
            string message = "Duplicate " + VendorSubscriber(vendor) + " Name Signed Up." + Environment.NewLine
                + Environment.NewLine
                + name;

            SendEmail(Config.FromEmail, Config.AdminEmail, "Duplicate " + VendorSubscriber(vendor) + " Name Signed Up - " + name, message);
        }

        public void SendAdminProfileEditedEmail(string name, bool vendor)
        {
            string message = VendorSubscriber(vendor) + " Edited ANY Profile Field." + Environment.NewLine
                + Environment.NewLine
                + name;

            SendEmail(Config.FromEmail, Config.AdminEmail, VendorSubscriber(vendor) + " Edited ANY Profile Field - " + name, message);
        }

        public void SendAdminSubscriptionCancelledEmail(string name)
        {
            string message = "Subscriber has cancelled subscription." + Environment.NewLine
                + Environment.NewLine
                + name;

            SendEmail(Config.FromEmail, Config.AdminEmail, "Subscription Cancelled - " + name, message);
        }
        #endregion

        #region account/user

        public bool SendConfirmationEmail(string email, string userName, string callbackUrl, bool passwordSet = true, string createdBy = "The Admin")
        {
            string message = (passwordSet ? userName + "," + Environment.NewLine + Environment.NewLine : "")
                + $"Welcome to {Config.PortalName}!" + Environment.NewLine
                + Environment.NewLine
                + (passwordSet ? "" : createdBy + $" just created your {Config.PortalName} user account." + Environment.NewLine + Environment.NewLine)
                + "Click below to verify your account:" + Environment.NewLine
                + callbackUrl + Environment.NewLine
                + Environment.NewLine
                + (passwordSet ? "" : "Your Username is " + email + Environment.NewLine + Environment.NewLine)
                + (passwordSet ? $"Thank you for creating an account with {Config.PortalName}" : Config.PortalName);

            return SendEmail(Config.FromEmail, email, $"{Config.PortalName} " + (passwordSet ? "Account" : "New User") + " Confirmation", message);
        }

        public void SendAccountActivated(string email, string name, bool vendor)
        {
            string message = name + "," + Environment.NewLine
                + Environment.NewLine
                + $"Thank You for confirming your {Config.PortalName} account." + Environment.NewLine
                + Environment.NewLine
                + (vendor
                    //? "As a Vendor, you may now upload your capabilities and workscope icons into the Elena's List platform." + Environment.NewLine + Environment.NewLine +
                    //"There is no cost to list capabilities & highlight any capabilities based on workscope icon."
                    ? (
                        "Vendors use our level playing field to list & highlight capabilities." + Environment.NewLine
                        + Environment.NewLine
                        + "View/Upload Capabilities " + Environment.NewLine
                        + "https://" + HttpContext.Current.Request.Url.Host + "/Vendor/UploadList" + Environment.NewLine
                        + Environment.NewLine
                        + "Customize RFQ Settings " + Environment.NewLine
                        + "https://" + HttpContext.Current.Request.Url.Host + "/Vendor" + Environment.NewLine
                        + Environment.NewLine
                        + "Add Users " + Environment.NewLine
                        + "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Users" + Environment.NewLine
                        + Environment.NewLine
                        + "Use Your Capability List or our Template " + Environment.NewLine
                        + "https://" + HttpContext.Current.Request.Url.Host + "/Content/dist/files/MROfinderCapabilityUploadTemplate.xlsx"
					) : (
                        "Search any ✈ part in seconds; find component MROs." + Environment.NewLine
                        + "https://" + HttpContext.Current.Request.Url.Host + Environment.NewLine
                        + Environment.NewLine
                        + Environment.NewLine
                        + "For Support:" + Environment.NewLine
                        + "support@MROFINDER.aero"
                    )
                //"You may now search for Component Vendor Providers on the Elena's List platform."
                ) + Environment.NewLine
                + Environment.NewLine
                + Config.PortalName;

            SendEmail(Config.FromEmail, email, $"{Config.PortalName} Account Activated", message);
        }

        public void SendFreeTrialActivated(string email, string name)
        {
            string message = name + "," + Environment.NewLine
                + Environment.NewLine
                + "Your 2 Week Trial has started." + Environment.NewLine
                + Environment.NewLine
                + $"You may now search for Component Vendors on {Config.PortalName}" + Environment.NewLine
                + Environment.NewLine
                + "Your subscription will begin in 2 weeks" + Environment.NewLine
                + Environment.NewLine
                + "View or Change Subscription: https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Charge" + Environment.NewLine
                + Environment.NewLine
                + Config.PortalName;

            SendEmail(Config.FromEmail, email, $"{Config.PortalName} Free Trial Activated", message);
        }

        public void SendAccountApproved(string email, string vendorName, string vendorID)
        {
            string message = vendorName + "," + Environment.NewLine
                + Environment.NewLine
                + $"{Config.PortalName} has approved your FREE Vendor account for 25 monthly searches." + Environment.NewLine
                + Environment.NewLine
                + "You may now search for component Vendors on our platform." + Environment.NewLine
                + Environment.NewLine
                + "Remember, unlimited listing of Capabilities & Workscope icons is always free." + Environment.NewLine
                + Environment.NewLine
                + Config.PortalName;

            SendEmail(Config.FromEmail, email, $"{Config.PortalName} Account Approved for Searched", message);
        }

        public void SendPasswordResetEmail(string email, string callbackUrl)
        {
			string message = "A password reset has been requested. To reset your password, click below:" + Environment.NewLine
			 + callbackUrl;
			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Password Reset", message);
		}
        #endregion

        #region vendor

        public void SendVendorRFQEmail(string vendorQuoteID, string vendorId)
		{
			var result = _context.VendorQuoteGetByID4(vendorQuoteID.ToNullableInt()).FirstOrDefault();
			
			if (result != null)
            {
				string subscriberName = result.SubscriberName;
				int subscriberID = result.SubscriberID;
				string subscriberPhone = result.SubscriberPhone;
				string userEmail = result.UserEmail;
				string vendorName = result.VendorName;
				string partNumber = result.PartNumber;

				var vendor = _context.Vendors.Find(vendorId.ToInt());

				string message = "RFQ for Part # " + partNumber + " from " + subscriberName + Environment.NewLine
					+ Environment.NewLine
					+ "Contact Direct or Login to Quote." + Environment.NewLine
					+ Environment.NewLine
					+ "Email: " + userEmail + Environment.NewLine
					+ "Phone: " + subscriberPhone + Environment.NewLine
					+ Environment.NewLine
					+ Environment.NewLine
					//+ "Click to instant quote: " + Environment.NewLine
					//+ "https://" + HttpContext.Current.Request.Url.Host + "/Vendor/InstantQuote?ID=" + Environment.NewLine
					//+ Environment.NewLine
					//+ Environment.NewLine
					+ "Login & view your RFQs:" + Environment.NewLine
					+ "https://" + HttpContext.Current.Request.Url.Host + "/Vendor/Quote?VendorID=" + vendorId + Environment.NewLine
					+ Environment.NewLine
					+ Environment.NewLine
					+ GetAlternatePartsLine(partNumber, subscriberID)
					+ $"Generated @ {Config.PortalName}";

				// TODO: change "to" to vendor's RFQ email
				var email = vendor.RFQEmail;
				if (string.IsNullOrEmpty(vendor.RFQEmail))
                {
					var subscriber = _context.UserGetByVendorID(vendorId.ToInt()).FirstOrDefault();
					email = subscriber.Email;
                }
					
				SendEmail(Config.FromEmail, email, $"{Config.PortalName} RFQ for " + partNumber + " from " + subscriberName, message, Config.AdminEmail);
			}
		}

		public void SendVendorUploadEmail(string vendorEmail, string vendorName, bool capabilityList)
		{
			string message = vendorName + "," + Environment.NewLine
				+ Environment.NewLine
				+ "You have uploaded a " + CapabilityMerit(capabilityList, Config.PortalCode) + $" list to the {Config.PortalName} platform." + Environment.NewLine
				+ Environment.NewLine
				+ $"{Config.PortalName} will approve your list shortly, and make it active throughout the platform." + Environment.NewLine
				+ Environment.NewLine
				//"Elena's List will approve and activate your list throughout the platform within 24 hours." + Environment.NewLine + Environment.NewLine +
				+ "You will receive a confirmation when your " + CapabilitiesMerits(capabilityList, Config.PortalCode) + " are live." + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, vendorEmail, $"{Config.PortalName} " + CapabilityMerit(capabilityList, Config.PortalCode) + " List Uploaded", message);
		}

		// TODO: ask bryan why not in use
		public void SendVendorUploadByAdminEmail(string vendorEmail, string vendorName, bool capabilityList)
		{
			string message = vendorName + "," + Environment.NewLine
				+ Environment.NewLine
				+ $"An Admin at {Config.PortalName} has uploaded your " + CapabilityMerit(capabilityList, Config.PortalCode) + " list." + Environment.NewLine
				+ Environment.NewLine
				+ "This happened because:" + Environment.NewLine
				+ Environment.NewLine
				+ "You recently uploaded a " + CapabilityMerit(capabilityList, Config.PortalCode) + $" list that {Config.PortalName} has re-formatted for optimal functionality on our platform." + Environment.NewLine
				+ Environment.NewLine
				+ "OR" + Environment.NewLine
				+ Environment.NewLine
				+ $"You recently spoke with an {Config.PortalName} representative and they have uploaded your " + CapabilityMerit(capabilityList, Config.PortalCode) + " list on your behalf." + Environment.NewLine
				+ Environment.NewLine
				+ $"{Config.PortalName} will approve your list shortly, and make your " + CapabilitiesMerits(capabilityList, Config.PortalCode) + " active throughout the platform." + Environment.NewLine
				+ Environment.NewLine
				+ "You will receive a confirmation once your " + CapabilitiesMerits(capabilityList, Config.PortalCode) + " are live." + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, vendorEmail, $"{Config.PortalName} " + CapabilityMerit(capabilityList, Config.PortalCode) + " List Uploaded by Elena's List", message);
		}

		public void SendVendorListApprovedEmail(string vendorEmail, string vendorName, bool capabilityList, string capabilities, string merits, int portalCode)
		{
			string message = vendorName + "," + Environment.NewLine
				+ Environment.NewLine
				+ "Your " + CapabilityMerit(capabilityList, portalCode) + " list has been approved." + Environment.NewLine
				+ Environment.NewLine
				+ "Your " + CapabilitiesMerits(capabilityList, portalCode) + " are now active throughout " + (portalCode == 0 ? "MROFinder.buddy.aero" : "FindParts.buddy.aero") + Environment.NewLine
				+ Environment.NewLine
				+ "Your Capability Stats:" + Environment.NewLine
				+ "# of " + CapabilitiesMerits(capabilityList, portalCode) + " listed - " + capabilities + Environment.NewLine
				+ Environment.NewLine
				+ "Click to view your Lists:" + Environment.NewLine
				+ "https://" + HttpContext.Current.Request.Url.Host + "/Vendor/UploadList" + Environment.NewLine
				+ Environment.NewLine
				//+ "Remember, you can always upload " + (capabilityList ? "" : "more ") + "Workscope icons to highlight your specific capabilities for FREE," + Environment.NewLine + Environment.NewLine
				+ "buddy.aero";

			SendEmail(Config.FromEmail, vendorEmail, (Config.PortalCode == 0 ? "MROFinder" : "FindParts") + " " + CapabilityMerit(capabilityList, portalCode) + " List Approved & Active", message);
		}

		#endregion

		#region subscriber

		public void SendSubscriberAdminNewUserEmail(string email, string newEmail, string subscriberName, string createdBy)
		{
			string message = subscriberName + "," + Environment.NewLine
				+ Environment.NewLine
				+ createdBy + " just added a new user account" + Environment.NewLine
				+ Environment.NewLine
				+ "The new user in your organization is: " + newEmail + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} New User Created - " + subscriberName, message);
		}

		public void SendSubscriberCancelledEmail(string email, string subscriberName)
		{
			string message = subscriberName + "," + Environment.NewLine
				+ Environment.NewLine
				+ $"You have cancelled your {Config.PortalName} search subscription." + Environment.NewLine
				+ Environment.NewLine
				+ "Your login information is still active and you may re-add a subscription anytime." + Environment.NewLine
				+ Environment.NewLine
				+ "View or Change Subscription:" + Environment.NewLine
				+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Charge" + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Subscription Cancelled", message);
		}

		public void SendSubscriberQuoteEmail(string vendorQuoteID)
		{
			var result = _context.VendorQuoteGetByID4(vendorQuoteID.ToNullableInt()).FirstOrDefault();
			
			if (result != null)
			{
				string subscriberName = result.SubscriberName;
				int subscriberID = result.SubscriberID;
				string userName = result.UserName;
				string userEmail = result.UserEmail;
				string vendorName = result.VendorName;
				string partNumber = result.PartNumber;
				DateTime dateCreated = result.DateCreated;
				DateTime? dateResponded = result.DateResponded;
				DateTime? dateNoQuote = result.DateNoQuote;
				int? testPrice = result.TestPrice;
				int testTAT = result.TestTAT;
				int? repairPrice = result.RepairPrice;
				int? repairPriceRangeLow = result.RepairPriceRangeLow;
				int? repairPriceRangeHigh = result.RepairPriceRangeHigh;
				int repairTAT = result.RepairTAT;
				int? overhaulPrice = result.OverhaulPrice;
				int? overhaulPriceRangeLow = result.OverhaulPriceRangeLow;
				int? overhaulPriceRangeHigh = result.OverhaulPriceRangeHigh;
				int overhaulTAT = result.OverhaulTAT;
				int? notToExceed = result.NotToExceed;
				bool flatRate = result.FlatRate == true;
				string quoteComments = result.QuoteComments;
				string currency = result.Currency;
				string currencySymbol = Currency.GetSymbol(currency);

				if (dateNoQuote.HasValue)
				{
					string message = vendorName + " NO QUOTED Part Number: " + partNumber + Environment.NewLine
						+ Environment.NewLine
						+ "Quote Type: NO QUOTE" + Environment.NewLine
						+ Environment.NewLine
						+ Environment.NewLine
						+ Environment.NewLine
						+ "NO QUOTE" + Environment.NewLine
						+ Environment.NewLine
						+ Environment.NewLine
						+ Environment.NewLine
						+ "Click below to view your Quotes:" + Environment.NewLine
						+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Quote" + Environment.NewLine
						+ Environment.NewLine
						+ "RFQ Date: " + dateCreated + Environment.NewLine
						+ "No Quote Date: " + dateNoQuote + Environment.NewLine
						+ Environment.NewLine
						+ GetAlternatePartsLine(partNumber, subscriberID)
						+ $"Generated @ {Config.PortalName}";

					SendEmail("robot@MROfinder.aero", userEmail, $"{Config.PortalName} No Quote for " + partNumber + " from " + vendorName, message);
				}
				else
				{
					//string message = userName + "," + Environment.NewLine
					string message = vendorName + " Quoted:" + Environment.NewLine
						+ "Part Number: " + partNumber + Environment.NewLine
						+ Environment.NewLine
						+ "Quote Type: " + (flatRate
							? "Flat Rate"
							: (repairPriceRangeLow == null && overhaulPriceRangeLow == null
								? "Average Price"
								: (repairPrice == null && overhaulPrice == null
									? "Price Range"
									: "Price Range & Average"
								)
							)
						) + Environment.NewLine
						+ Environment.NewLine
						+ GetQuoteLine("Test", testTAT, currencySymbol, testPrice, null, null, true)
						+ GetQuoteLine("Repair", repairTAT, currencySymbol, repairPrice, repairPriceRangeLow, repairPriceRangeHigh, flatRate)
						+ GetQuoteLine("Overhaul", overhaulTAT, currencySymbol, overhaulPrice, overhaulPriceRangeLow, overhaulPriceRangeHigh, flatRate)
						+ (notToExceed.HasValue ? Environment.NewLine + "Not To Exceed: " + currencySymbol + notToExceed + Environment.NewLine : "")
						+ Environment.NewLine
						+ (quoteComments != "" ? "Quote Comments: " + quoteComments + Environment.NewLine + Environment.NewLine : "")
						+ Environment.NewLine
						+ "RFQ Date: " + dateCreated + Environment.NewLine
						+ "Quote Date: " + dateResponded + Environment.NewLine
						+ Environment.NewLine
						+ "Click below to view your Quotes:" + Environment.NewLine
						+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Quote" + Environment.NewLine
						+ Environment.NewLine
						+ GetAlternatePartsLine(partNumber, subscriberID)
						+ $"Generated @ {Config.PortalName}";
					;

					SendEmail(Config.FromEmail, userEmail, $"{Config.PortalName} Quote for " + partNumber + " from " + vendorName, message);
				}
			}
		}

		private static string GetQuoteLine(string type, int? TAT, string currencySymbol, int? price, int? priceRangeLow, int? priceRangeHigh, bool flatRate)
		{
			if (priceRangeLow.HasValue && price.HasValue)
			{
				return TAT + " Day " + type + " @ " + currencySymbol + priceRangeLow + " - " + currencySymbol + priceRangeHigh + " ( " + (flatRate ? "" : "Avg. ") + currencySymbol + price + " )" + Environment.NewLine;
			}
			else if (priceRangeLow.HasValue)
			{
				return TAT + " Day " + type + " @ " + currencySymbol + priceRangeLow + " - " + currencySymbol + priceRangeHigh + Environment.NewLine;
			}
			else if (price.HasValue)
			{
				return TAT + " Day " + type + " @ " + (flatRate ? "" : "Avg. ") + currencySymbol + price + Environment.NewLine;
			}
			return string.Empty;
		}

		private string GetAlternatePartsLine(string partNumber, int subscriberID)
		{
			var alternates = new List<string>();
			var result = _context.VendorListItemSearchAlternateDetail(partNumber, subscriberID).ToList();
			foreach (var altPart in result)
            {
				string alternatePartNumber = altPart.AlternatePartNumber;
				string alternatePartNumber2 = altPart.AlternatePartNumber2;
				string modelNumber = altPart.ModelNumber;

				if (!string.IsNullOrEmpty(alternatePartNumber) && !alternates.Contains(alternatePartNumber))
				{
					alternates.Add(alternatePartNumber);
				}
				if (!string.IsNullOrEmpty(alternatePartNumber2) && !alternates.Contains(alternatePartNumber2))
				{
					alternates.Add(alternatePartNumber2);
				}
				if (!string.IsNullOrEmpty(modelNumber) && !alternates.Contains("Model # " + modelNumber))
				{
					alternates.Add("Model # " + modelNumber);
				}
			}
		
			if (alternates.Count > 0)
			{
				return "Possible Alternate P/Ns: " + string.Join(", ", alternates) + Environment.NewLine + Environment.NewLine;
			}
			return string.Empty;
		}

		#endregion

		#region subscriber stripe

		public void SendInvoiceCreatedEmail(string name, string email, string amount)
		{
			/*
			string message = name + "," + Environment.NewLine 
				+ Environment.NewLine 
				+ "We have processed your membership payment in the amount of " + amount + Environment.NewLine 
				+ Environment.NewLine 
				+ "Please view payment history and invoices here:" + Environment.NewLine
				+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Charge" + Environment.NewLine
				+ Environment.NewLine
				+ "Elena's List";
			 */
			string message = name + "," + Environment.NewLine
				+ Environment.NewLine
				+ $"We have received payment for your {Config.PortalName} subscription. " + Environment.NewLine
				+ Environment.NewLine
				+ "Total Paid: " + amount + Environment.NewLine
				+ Environment.NewLine
				+ "Please view payment history and invoices here:" + Environment.NewLine
				+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Charge" + Environment.NewLine
				+ Environment.NewLine
				+ $"This charge will appear on your card as \"{Config.PortalName} - SEARCH SUBSCRIPTION - P. +1-949-335-3555\"" + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;
			string bcc = "alerts@mrofinder.aero";

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Subscription payment receipt", message, bcc);
		}

		public void SendFailedTransactionEmail(string name, string email)
		{
			string message = name + Environment.NewLine
				+ Environment.NewLine
				+ $"Recently, {Config.PortalName} merchant processor STRIPE attempted to run membership charge on your credit card payment method." + Environment.NewLine
				+ Environment.NewLine
				+ "This charge was declined. We will re-attempt to process this charge in the next few days." + Environment.NewLine
				+ Environment.NewLine
				+ "Please contact us if you prefer to make other arrangements." + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Failed STRIPE transactional charge " + name, message);
		}

		public void SendUpDowngradeEmail(string name, string email, bool upgrade)
		{
			string message = name + Environment.NewLine
				+ Environment.NewLine
				+ "This is an auto generated email that confirms you have successfully " + (upgrade ? "upgraded" : "downgraded") + $" your {Config.PortalName} membership." + Environment.NewLine
				+ Environment.NewLine
				+ "Please view payment history and invoices here:" + Environment.NewLine
				+ "https://" + HttpContext.Current.Request.Url.Host + "/Subscriber/Charge" + Environment.NewLine
				+ Environment.NewLine
				+ Config.PortalName;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Membership " + (upgrade ? "Upgraded" : "Downgraded"), message);
		}

		#endregion

		#region admin stripe

		public void SendAdminFailedTransactionEmail(string name)
		{
			string message = "Failed STRIPE transactional charge on " + DateTime.Now + Environment.NewLine
				+ Environment.NewLine
				+ name;

			SendEmail(Config.FromEmail, Config.AdminEmail, "Failed STRIPE transactional charge " + name, message);
		}

		public void SendAdminUpDowngradeEmail(string name, bool upgrade)
		{
			string message = "Subscriber has " + (upgrade ? "upgraded" : "downgraded") + " membership level" + Environment.NewLine
				+ Environment.NewLine
				+ name;

			SendEmail(Config.FromEmail, Config.AdminEmail, "Membership " + (upgrade ? "Upgraded" : "Downgraded") + " - " + name, message);
		}

		#endregion

		public void SendDisabledFeatureEmail(string email)
		{
			string message = "Subscriber search feature disabled, entered email: " + email;
			SendEmail(Config.FromEmail, Config.AdminEmail, message, message);
		}

		public void SendMessageUsEmail(string name, string contact, string body)
		{
			string message = "Message Us: " + Environment.NewLine
				+ Environment.NewLine
				+ "Name / Company: " + name + Environment.NewLine
				+ "Email / Phone: " + contact + Environment.NewLine
				+ "Write to us: " + body + Environment.NewLine;

			SendEmail(Config.FromEmail, Config.AdminEmail, "Message Us: " + name, message, "mro@mrofinder.aero");
		}

		public void SendWebhookEmail(string eventType, string json)
		{
			string email = Config.DevEmail;
			string message = json;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Webhook: " + eventType, message);
		}

        public void SendJitsiMeetingInvitationEmail(string sender, string email, string meetingUrl)
        {
			string message = $"{sender} is inviting you to a meeting."
				+ Environment.NewLine
				+ Environment.NewLine
				+ "Join the meeting:"
				+ Environment.NewLine
				+ meetingUrl
				+ Environment.NewLine;

			SendEmail(Config.FromEmail, email, $"{Config.PortalName} Meet Invite", message, Config.AdminEmail);
		}

        public void SendStripeChargeFailedEmail(string email, string vendorName)
        {
			string message = $"{email}"
                + Environment.NewLine
				+ $"{vendorName}"
                + Environment.NewLine
				+ "Recently STRIPE attempted to run a buddy.aero charge on your credit card."
				+ Environment.NewLine
				+ "This charge was declined. We may re-attempt to process this charge in the next few days."
				+ Environment.NewLine
				+ "Please contact us if you prefer to make other arrangements."
				+ Environment.NewLine
				+ "Please view payment history and invoices here:"
				+ Environment.NewLine
				+ "https://findparts.buddy.aero/Subscriber"
				+ Environment.NewLine
				+ Environment.NewLine
				+ Environment.NewLine
				+ "buddy.aero"
				+ Environment.NewLine
				+ "+1-949-290-7212";

			SendEmail(Config.FromEmail, email, $"Payment Transaction Failed", message, Config.DevEmail);
		}

        public void SendStripeChargeSucceededEmail(string email, string vendorName, long amount)
        {
			string message = $"{email}"
				+ Environment.NewLine
				+ $"{vendorName}"
				+ Environment.NewLine
				+ $"We have processed your payment for LinkedIn Impressions in the amount of {((double)amount / 100).ToString("C2")}"
				+ Environment.NewLine
				+ "Please view payment history and invoices here:"
				+ Environment.NewLine
				+ "https://findparts.buddy.aero/Subscriber"
				+ Environment.NewLine
				+ "This charge will appear on your card as buddy.aero"
				+ Environment.NewLine
				+ Environment.NewLine
				+ Environment.NewLine
				+ "buddy.aero"
				+ Environment.NewLine
				+ "+1-949-290-7212";

			SendEmail(Config.FromEmail, email, $"Payment Receipt", message, Config.DevEmail);
		}

        public void SendWelcomeEmail(string userName, string email)
        {
			string message = 
				$"{(!string.IsNullOrEmpty(userName) ? userName : email)}"
				+ "<br/><br/>"
				+ $"Thank you for creating an account on buddy.aero. "
				+ "<br/><br/>"
				+ $"Where Aviation Professionals Interview Aviation Brands."
				+ "<br/><br/>"
				+ @"For ✈️ Brands: <a href=""https://calendly.com/aero-celebrity-podcast"">Book a Mini Podcast</a>"
				+ "<br/>"
				+ @"For ✈️ Creators: <a href=""https://docs.google.com/forms/d/e/1FAIpQLSfP-7FfvdhWNFKLd3jeCkwB3OVqxrSh2VjYWoCrlbkbKvuzlQ/viewform"">Get Paid to Share on LinkedIn</a>"
				+ "<br/>"
				+ "<br/>"
				+ "<br/>"
				+ "buddy.aero"
				+ "<br/>"
				+ "+1-949-290-7212";

			SendEmail(Config.FromEmail, email, $"Welcome to buddy.aero", message, Config.AdminEmail, true);
		}
    }
}