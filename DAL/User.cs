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
    using System.Collections.Generic;
    
    public partial class User
    {
        public int UserID { get; set; }
        public System.Guid ProviderUserKey { get; set; }
        public Nullable<int> SubscriberID { get; set; }
        public Nullable<int> VendorID { get; set; }
        public string Email { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateActivated { get; set; }
        public bool Verified { get; set; }
        public bool Active { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string PersonalEmail { get; set; }
        public string SkypeName { get; set; }
        public string ResetPasswordToken { get; set; }
        public Nullable<System.DateTime> ResetPasswordTime { get; set; }
        public Nullable<int> CreatedByUserID { get; set; }
        public Nullable<System.DateTime> DateDeleted { get; set; }
    }
}