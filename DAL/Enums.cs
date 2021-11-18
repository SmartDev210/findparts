using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
		[Display(Name = "Advertise")]
		Advertise = 0,
		[Display(Name = "Organic all impressions")]
		OrganicAllImpressions = 1,
		[Display(Name = "Organic target impressions")]
		OrganicTargetImpressions = 2,
		[Display(Name = "Sponsored target impressions")]
		SponsoredTargetImpressions = 3
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
	public static class EnumExtensions
	{
		public static string GetDisplayName(this Enum enumValue)
		{
			return enumValue.GetType()
							.GetMember(enumValue.ToString())
							.First()
							.GetCustomAttribute<DisplayAttribute>()
							.GetName();
		}
	}
}
