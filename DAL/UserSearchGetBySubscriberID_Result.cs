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
    
    public partial class UserSearchGetBySubscriberID_Result
    {
        public string Email { get; set; }
        public int UserSearchID { get; set; }
        public int UserID { get; set; }
        public string SearchTerm { get; set; }
        public bool PartPage { get; set; }
        public int ResultCount { get; set; }
        public bool ExactMatch { get; set; }
        public System.DateTime DateCreated { get; set; }
        public int RepeatCount { get; set; }
    }
}
