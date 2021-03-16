using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class MailService : IMailService
    {
        public void SendAdminNewAccountEmail(string companyName, bool v, string planSelected)
        {
            throw new NotImplementedException();
        }

        public void SendDisabledFeatureEmail(string disabledFeatureEmail)
        {
            throw new NotImplementedException();
        }

        public void SendVendorRFQEmail(string vendorQuoteId)
        {
            throw new NotImplementedException();
        }
    }
}