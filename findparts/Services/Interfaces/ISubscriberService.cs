﻿using DAL;
using Findparts.Models.Subscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface ISubscriberService
    {
        SubscriberIndexPageViewModel GetSubscriberIndexPageViewModel(int? subscriberId);
        List<UserGetBySubscriberID_Result> GetUsersViewModel(string subscriberId);
        SubscriberNewUserViewModel GetSubscriberNewUserViewModel(string subscriberId);
        int? GetVendorIdFromSubscriberId(string subscriberId);
    }
}
