using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IMailService
    {
        void SendVendorRFQEmail(string vendorQuoteId);
        void SendDisabledFeatureEmail(string disabledFeatureEmail);
        void SendAdminNewAccountEmail(string companyName, bool v, string planSelected);
    }
}
