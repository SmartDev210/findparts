using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IMailService
    {
        void SendAdminUploadEmail(string vendorName, string vendorID, bool capabilityList);
        void SendAdminNewAccountEmail(string companyName, bool vendor, string planSelected);
        void SendAdminNewUserEmail(string email, string subscriberName, string createdBy);
        void SendAdminAccountActivatedEmail(string name, bool vendor);
        void SendAdminDuplicateSignupEmail(string name, bool vendor);
        void SendAdminProfileEditedEmail(string name, bool vendor);
        void SendAdminSubscriptionCancelledEmail(string name);
        bool SendConfirmationEmail(string email, string userName, string callbackUrl, bool passwordSet = true, string createdBy = "The Admin");
        void SendAccountActivated(string email, string name, bool vendor);
        void SendFreeTrialActivated(string email, string name);
        void SendAccountApproved(string email, string vendorName, string vendorID);
        void SendVendorRFQEmail(string vendorQuoteID);
        void SendVendorUploadEmail(string vendorEmail, string vendorName, bool capabilityList);
        void SendVendorUploadByAdminEmail(string vendorEmail, string vendorName, bool capabilityList);
        void SendVendorListApprovedEmail(string vendorEmail, string vendorName, bool capabilityList, string capabilities, string merits);
        void SendSubscriberAdminNewUserEmail(string email, string newEmail, string subscriberName, string createdBy);
        void SendSubscriberCancelledEmail(string email, string subscriberName);
        void SendSubscriberQuoteEmail(string vendorQuoteID);
        void SendInvoiceCreatedEmail(string name, string email, string amount);
        void SendFailedTransactionEmail(string name, string email);
        void SendUpDowngradeEmail(string name, string email, bool upgrade);
        void SendAdminFailedTransactionEmail(string name);
        void SendAdminUpDowngradeEmail(string name, bool upgrade);
        void SendDisabledFeatureEmail(string email);
        void SendMessageUsEmail(string name, string contact, string body);
        void SendWebhookEmail(string eventType, string json);
    }
}
