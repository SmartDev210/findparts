using DAL;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Services.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly FindPartsEntities _context;
        private readonly FindPartsIdentityEntities _identityContext;

        public MembershipService(FindPartsEntities context, FindPartsIdentityEntities identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public Subscriber GetSubscriberById(string subscriberId)
        {
            int id = 0;
            if (Int32.TryParse(subscriberId, out id)) {
                var result = _context.SubscriberGetByID(id).ToList();
                if (result.Count > 0)
                {
                    return result[0];
                }
            }
            return null;
        }
    }
}