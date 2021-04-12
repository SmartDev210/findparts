using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Findparts.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime FromUnixTime(this uint unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static double ToUnixTime(this DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = (value - epoch);

            //return the total seconds (which is a UNIX timestamp)
            return span.TotalSeconds;
        }

        public static double ToUnixTime(this DateTime? value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            if (value != null)
            {
                DateTime x = value.Value;
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan span = (x - epoch);

                //return the total seconds (which is a UNIX timestamp)
                return span.TotalSeconds;
            }
            return 0;
        }

        public static String FormatDate(this DateTime dt)
        {
            string newDate = string.Format("{0:yyyy-MM-dd HH:mm:ss} GMT {1}", dt.ToLocalTime(),
                dt.ToLocalTime().ToString("%K"));
            return newDate;
        }

        public static DateTime MonthStartDate(this DateTime value)
        {
            DateTime monthStart = new DateTime(value.Date.Year, value.Date.Month, 1);
            return monthStart;
        }

        public static DateTime LastWorkingDay(this DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return date.AddDays(-2);
                case DayOfWeek.Monday:
                    return date.AddDays(-3);
                default:
                    return date.AddDays(-1);
            }
        }
    }
}