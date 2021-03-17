using DAL;
using Findparts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IMembershipService
    {
        Subscriber GetSubscriberById(string subscriberId);
        string RegisterNewUser(RegisterViewModel model, ApplicationUser user);
        void PopulateRegisterViewModel(RegisterViewModel viewModel);
        void ApproveUser(ApplicationUser user, bool primaryUser);
    }
}
