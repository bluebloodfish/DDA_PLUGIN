using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public static class DateTime_Tool
    {
        public static long Get(DateTime dateTime)
        {
            return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static long GetDUCTimeStamp(DateTime dateTime)
        {
            return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static long GetUnixTimestamp(DateTime dateTime)
        {
            var timeSpan = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (long)timeSpan.TotalSeconds;
        }

        public static DateTime GetDateTime(long unixTimeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(unixTimeStamp).ToLocalTime();
            return dt;
        }

    }
}
