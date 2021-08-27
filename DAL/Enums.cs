using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public enum SubscriberTypeID
	{
		StarterMonthly = 5,
		StandardMonthly = 6,
		EnterpriseMonthly = 7,
		NoCreditCard = 8,
		StarterAnnual = 9,
		StandardAnnual = 10,
		EnterpriseAnnual = 11
	}
	public enum PurchaseType
    {
		Advertise = 0,
		Impressions = 1
    }
	/*
	public enum StatusID
	{
		BrandNew = 1,
		PendingMRO = 11,
		ApprovedMRO = 5,
		FreeTrialingNew = 14,
		FreeTrialingPurge = 15,
		ApprovedSubscriberStripe = 10,
		ApprovedSubscriberOther = 2,
		Spam = 9,
		DoNotApprove = 4,
		SubscriptionExpiredStripe = 12,
		SubscriptionExpiredOther = 13
	}
	*/
}
