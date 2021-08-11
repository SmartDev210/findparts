using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Subscriber
{
    public class UsersViewModel
    {
        public UsersViewModel()
        {
            Users = new List<UserGetBySubscriberID_Result>();
        }
        public List<UserGetBySubscriberID_Result> Users { get; set; }
        public int? WeavyCompanyId { get; set; }
    }
}