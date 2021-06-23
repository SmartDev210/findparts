using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IJitsiService
    {
        List<string> GetVendorUserList(int vendorId);
        void SendInvitiationEmails(int vendorId, string userEmail, string meetUrl);
    }
}
