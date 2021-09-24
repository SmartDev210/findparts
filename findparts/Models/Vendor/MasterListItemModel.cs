using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models
{
    public class MasterListItemModel
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string AlternatePartNumber { get; set; }
        public string AlternatePartNumber2 { get; set; }
        public string ModelNumber { get; set; }
        public string Manufacturer { get; set; }
        public string ATAChapter { get; set; }
        public string NSN { get; set; }
        public string Aircraft { get; set; }
        public string Engine { get; set; }
        public string Cage { get; set; }
        public Nullable<bool> PMA { get; set; }
        public Nullable<bool> DER { get; set; }
        public Nullable<bool> RepairsFrequently { get; set; }
        public Nullable<bool> FreeEval { get; set; }
        public Nullable<bool> Modified { get; set; }
        public Nullable<bool> FunctionTestOnly { get; set; }
        public Nullable<bool> NoOverhaulWorkscope { get; set; }
        public Nullable<bool> CAAC { get; set; }
        public Nullable<bool> ExtendedWarranty { get; set; }
        public Nullable<bool> FlatRate { get; set; }
        public Nullable<bool> Range { get; set; }
        public Nullable<bool> NTE { get; set; }
        public string NotesRemarks { get; set; }
        public string WorkShopSite { get; set; }
        public string Workscope { get; set; }
        public string Quantity { get; set; }
        public string Condition { get; set; }
        public string Serial { get; set; }
    }
}