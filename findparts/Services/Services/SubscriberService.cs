using DAL;
using Findparts.Models.Subscriber;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class SubscriberService : ISubscriberService
    {
        private readonly FindPartsEntities _context;
        public SubscriberService(FindPartsEntities context)
        {
            _context = context;
        }

        public SubscriberIndexPageViewModel GetSubscriberIndexPageViewModel(int? subscriberId)
        {
            var viewModel = new SubscriberIndexPageViewModel();
            var result = _context.SubscriberGetStatsByID(subscriberId).FirstOrDefault();
            if (result != null)
            {
                viewModel.SubscriberName = result.SubscriberName;
                viewModel.Country = result.Country;
                viewModel.SubscriberType = result.SubscriberType;
                if (viewModel.SubscriberType == "NoCreditCard")
                    viewModel.SubscriberType = "No Active Plan";
                viewModel.UserCount = result.UserCount ?? 0;
                viewModel.UserQuota = result.UserQuota;

                viewModel.SearchCount = result.SearchCount ?? 0;
                viewModel.SearchQuota = result.SearchQuota;
            }
            return viewModel;
        }
    }
}