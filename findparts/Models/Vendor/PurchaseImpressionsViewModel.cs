using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Models.Vendor
{
    public class PurchaseImpressionsViewModel
    {
        public int MoneyPer1000Views { get; set; }
        public PurchaseType PurchaseType { get; set; }
    }
}