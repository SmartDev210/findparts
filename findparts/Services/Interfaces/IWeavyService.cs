using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IWeavyService
    {   
        Task<int> GetMemberId(int vendorId);
        string GetWeavyToken(string userId, string email);
        Task<int> GetCollabChannel(int vendorId, string userId);
        Task<int> GetServiceRequestChannel(int vendorId, string userId);
    }
}
