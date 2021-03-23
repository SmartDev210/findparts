using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Models.Subscriber
{
    public enum SubscriptionUpdateMode
    {
        Create = 0,
        UpdatePaymentInfo = 1,
        UpdatePlan = 2
    }
    public class SubscriberChargeViewModel
    {
        public SubscriberChargeViewModel()
        {
            PlanSelectList = new List<SelectListItem>();
        }
      
        public List<SelectListItem> PlanSelectList { get; set; }

        public int SubscriberTypeId { get; set; }

        public SubscriptionUpdateMode SubscriptionUpdateMode { get; set; }
    }
    public class SubscriberChargeInfoViewModel
    {
        public SubscriberChargeInfoViewModel()
        {
            Invoices = new List<SubscriberInvoice>();
        }
        public string CardLastFour { get; set; }
        public string PlanText { get; set; }
        public string SubscriptionStatus { get; set; }
        public string SubscriptionPeriod { get; set; }
        public string ListSubscriptionPendingActionText { get; set; }

        public bool HasSubscription { get; set; }

        public string StripeSubscriptionId { get; set; }
        public string StripePlanId { get; set; }

        public List<SubscriberInvoice> Invoices { get; set; }
    }
}