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
    
    public partial class VendorQuoteGetByUserID_Result
    {
        public string SubscriberName { get; set; }
        public string UserName { get; set; }
        public string VendorName { get; set; }
        public int VendorQuoteID { get; set; }
        public string PartNumber { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> DateResponded { get; set; }
        public Nullable<int> TestPrice { get; set; }
        public int TestTAT { get; set; }
        public Nullable<int> RepairPrice { get; set; }
        public int RepairTAT { get; set; }
        public Nullable<int> OverhaulPrice { get; set; }
        public int OverhaulTAT { get; set; }
        public Nullable<bool> WorkHistory { get; set; }
        public Nullable<bool> PMA { get; set; }
        public Nullable<bool> DER { get; set; }
        public Nullable<bool> FreeEval { get; set; }
        public Nullable<bool> APR { get; set; }
        public Nullable<bool> FT { get; set; }
    }
}
