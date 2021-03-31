using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Findparts.Extensions
{
    public static class Extensions
    {
        public static int? ToNullableInt(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            int t;
            if (Int32.TryParse(str, out t)) return t;
            return null;
        }
        public static int ToInt(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            int t = 0;
            if (Int32.TryParse(str, out t)) return t;
            return 0;
        }
        public static int? ToNullableInt(this decimal? d)
        {
            if (d == null) return null;
            return (int)d.Value;
        }
        public static string GetContentType(this string fileType)
        {
            if (fileType.EndsWith(".xls"))
            {
                return "application/vnd.ms-excel";
            } else if (fileType.EndsWith(".xlsx"))
            {
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            } else if (fileType.EndsWith(".csv"))
            {
                return "text/csv";
            } else if (fileType.EndsWith(".pdf"))
            {
                return "application/pdf";
            } else
            {
                return "application/octet-stream";
            }
        }
    }
}