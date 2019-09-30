using System;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    public static class TimeZoneInfoExtensions
    {
        public static TimeZoneInfo GetGmtTimeZone()
        {
            return GetByStandardName("GMT Standard Time");
        }

        public static TimeZoneInfo GetByStandardName(string standardName)
        {
            Guard.NotNullOrEmpty("standardName", standardName);

            return TimeZoneInfo.GetSystemTimeZones().Single(t => t.StandardName == standardName);
        }
    }
}
