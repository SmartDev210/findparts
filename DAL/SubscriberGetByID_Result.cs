//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    
    public partial class SubscriberGetByID_Result
    {
        public int SubscriberID { get; set; }
        public int SubscriberTypeID { get; set; }
        public bool Active { get; set; }
        public int StatusID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateTrialStart { get; set; }
        public string SubscriberName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string PatronType { get; set; }
        public string AccountingFirstName { get; set; }
        public string AccountingLastName { get; set; }
        public string AccountingPhone { get; set; }
        public string AccountingEmail { get; set; }
        public int ProfileFieldsCompleted { get; set; }
        public string Notes { get; set; }
        public string StripeCustomerID { get; set; }
        public Nullable<int> PendingSubscriberTypeID { get; set; }
        public Nullable<int> SignupSubscriberTypeID { get; set; }
    }
}
