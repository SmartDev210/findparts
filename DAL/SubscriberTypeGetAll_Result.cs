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
    
    public partial class SubscriberTypeGetAll_Result
    {
        public int SubscriberTypeID { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public int SearchQuota { get; set; }
        public int UserQuota { get; set; }
        public int Price { get; set; }
        public string StripePlanID { get; set; }
        public bool Annual { get; set; }
        public bool Standard { get; set; }
        public bool InstantAccess { get; set; }
        public int Level { get; set; }
    }
}
