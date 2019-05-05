using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ByteBank.ForumNaviteFramework.Utils
{
    public class HelpDate
    {
        public static DateTime GetDateTimeZoneBr(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }

    }
}