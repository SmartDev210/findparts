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
    
    public partial class UserSearch
    {
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
