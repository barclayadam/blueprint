using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Provides a number of extension methods of the built-in <see cref="DateTime"/> struct.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Given a DateTime will get the last second of the day that it represents, with ticks set to 0,
        /// that is 23:59:59.000.
        /// </summary>
        /// <param name="dateTime">The date time from which to get the end of the day.</param>
        /// <returns>The 'end of the day' of the given DateTime instance.</returns>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// Returns a value indicating whether of not this date is in the future, based on the
        /// value of <see cref="SystemTime.UtcNow"/>.
        /// </summary>
        /// <param name="date">
        /// The date to check.
        /// </param>
        /// <param name="temporalCheckType">The type of check to perform.</param>
        /// <returns>Whether the given date is in the future.</returns>
        public static bool IsInFuture(this DateTime date, TemporalCheck temporalCheckType)
        {
            if (temporalCheckType == TemporalCheck.DateTime)
            {
                return date > SystemTime.UtcNow;
            }

            return date.Date > SystemTime.UtcNow.Date;
        }

        /// <summary>
        /// Returns a value indicating whether of not this date is in the past, based on the
        /// value of <see cref="SystemTime.UtcNow"/>.
        /// </summary>
        /// <param name="date">
        /// The date to check.
        /// </param>
        /// <param name="temporalCheckType">The type of check to perform.</param>
        /// <returns>Whether the given date is in the past.</returns>
        public static bool IsInPast(this DateTime date, TemporalCheck temporalCheckType)
        {
            if (temporalCheckType == TemporalCheck.DateTime)
            {
                return date < SystemTime.UtcNow;
            }

            return date.Date < SystemTime.UtcNow.Date;
        }

        /// <summary>
        /// Gets the date of the previous valid day.
        /// </summary>
        /// <param name="date">The date to find the previous date from.</param>
        /// <param name="validDays">A list of valid days to select from.</param>
        /// <returns>The previous <see cref="DateTime" /> that is one of the specified days _and_ before <paramref name="date"/>.</returns>
        public static DateTime GetPreviousDate(this DateTime date, IEnumerable<DayOfWeek> validDays)
        {
            var dayOfWeeks = validDays as IList<DayOfWeek> ?? validDays.ToList();

            if (!dayOfWeeks.Any())
            {
                throw new ArgumentException("validDays");
            }

            date = date.AddDays(-1);

            while (!dayOfWeeks.Contains(date.DayOfWeek))
            {
                date = date.AddDays(-1);
            }

            return date;
        }

        /// <summary>
        /// Converts from UNIX timestamp (seconds) to a DateTime.
        /// </summary>
        /// <param name="unixTime">The UNIX timestamp to convert.</param>
        /// <returns>The equivalent DateTime.</returns>
        public static DateTime FromUnixTimeSeconds(this long unixTime)
        {
            return UnixEpoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Converts from UNIX timestamp (milliseconds) to a DateTime.
        /// </summary>
        /// <param name="unixTime">The UNIX timestamp to convert.</param>
        /// <returns>The equivalent DateTime.</returns>
        public static DateTime FromUnixTimeMilliseconds(this long unixTime)
        {
            return UnixEpoch.AddMilliseconds(unixTime);
        }

        /// <summary>
        /// Converts from a DateTime to UNIX timestamp (seconds).
        /// </summary>
        /// <param name="date">The DateTime to convert.</param>
        /// <returns>The equivalent UNIX timestamp.</returns>
        public static long ToUnixTimeSeconds(this DateTime date)
        {
            return Convert.ToInt64((date - UnixEpoch).TotalSeconds);
        }

        /// <summary>
        /// Converts from a DateTime to UNIX timestamp (milliseconds).
        /// </summary>
        /// <param name="date">The DateTime to convert.</param>
        /// <returns>The equivalent UNIX timestamp.</returns>
        public static long ToUnixTimeMilliseconds(this DateTime date)
        {
            return Convert.ToInt64((date - UnixEpoch).TotalMilliseconds);
        }
    }
}
