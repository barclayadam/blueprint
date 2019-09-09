using System;

namespace Blueprint.Core
{
    /// <summary>
    /// Provides an abstraction over DateTime, and in particular the current time, to allow easier testing
    /// of time-sensitive functions.
    /// </summary>
    /// <remarks>
    /// Any function that requires access to the current time should instead use the <see cref="UtcNow"/>
    /// property as tests can change this, unlike if DateTime.Now was used, which can not be mocked out and
    /// manipulated.
    /// </remarks>
    public static class SystemTime
    {
        private static Func<DateTime> getUtcNow = () => DateTime.UtcNow;
        private static Func<DateTime> getNow = () => DateTime.Now;

        /// <summary>
        /// Gets the current date and time, as defined by the current <see cref="getUtcNow"/> method, which in
        /// a production environment would simply delegate to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public static DateTime UtcNow => getUtcNow();

        /// <summary>
        /// Gets the current date and time, as defined by the current <see cref="getNow"/> method, which in
        /// a production environment would simply delegate to <see cref="DateTime.Now"/>.
        /// </summary>
        public static DateTime Now => getNow();

        /// <summary>
        /// Starts 'timelord mode' where it is possible to control time for any client that
        /// uses the <see cref="SystemTime" /> properties to get the current time.
        /// </summary>
        /// <remarks>
        /// This method will set the time to a static value the same as <see cref="DateTime.UtcNow" />
        /// at the time of execution.
        /// </remarks>
        /// <returns>A <see cref="SystemTimeModifier" /> which allows for modification of time.</returns>
        public static SystemTimeModifier PlayTimelord()
        {
            var staticTime = DateTime.UtcNow;

            getUtcNow = () => staticTime;
            getNow = () => staticTime;

            return new SystemTimeModifier();
        }

        /// <summary>
        /// Restores <see cref="UtcNow"/> to its default of returning <see cref="DateTime.UtcNow"/>.
        /// Restores <see cref="Now"/> to its default of returning <see cref="DateTime.Now"/>.
        /// </summary>
        public static void RestoreDefault()
        {
            getUtcNow = () => DateTime.UtcNow;
            getNow = () => DateTime.Now;
        }

        public class SystemTimeModifier : IDisposable
        {
            /// <summary>
            /// Gets the current date and time, as defined by the current <see cref="getUtcNow"/> method, which in
            /// a production environment would simply delegate to <see cref="DateTime.UtcNow"/>.
            /// </summary>
            public DateTime UtcNow => SystemTime.UtcNow;

            /// <summary>
            /// Gets the current date and time, as defined by the current <see cref="getNow"/> method, which in
            /// a production environment would simply delegate to <see cref="DateTime.Now"/>.
            /// </summary>
            public DateTime Now => SystemTime.Now;

            public DateTime SetUtcNow(DateTime date)
            {
                getUtcNow = () => date;

                return date;
            }

            public DateTime FastForwardUtc(TimeSpan timeSpan)
            {
                var newDate = UtcNow.Add(timeSpan);

                getUtcNow = () => newDate;

                return newDate;
            }

            public DateTime RewindUtc(TimeSpan timeSpan)
            {
                var newDate = UtcNow.Add(-timeSpan);

                getUtcNow = () => newDate;

                return newDate;
            }

            public void Dispose()
            {
                RestoreDefault();
            }
        }
    }
}