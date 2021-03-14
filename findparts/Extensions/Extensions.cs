using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}