using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models
{
    public class PartsPageViewModel
    {
        public PartsPageViewModel()
        {
            SearchResults = new List<VendorListItemSearch7_Result>();
            SimiliarSearches = new List<VendorListItemSearchSimilar_Result>();
        }
        public string Text { get; set; }
        public bool HasMoreRow { get; set; }
        public int RealCount { get; set; }
        public List<VendorListItemSearch7_Result> SearchResults { get; set; }
        public List<VendorListItemSearchSimilar_Result> SimiliarSearches { get; set; }
        public string[][] Merit { get; set; }
    }

    public class PartAutoComplete
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}