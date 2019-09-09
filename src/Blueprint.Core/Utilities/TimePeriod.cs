using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// A time period, defined simply as a start and end date.
    /// </summary>
    public class TimePeriod
    {
        /// <summary>
        /// Initializes a new instance of the TimePeriod class.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public TimePeriod(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        /// Gets the end date of this time period.
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Gets the start date of this time period.
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Returns all of the TimePeriods that are overlapped by this instance.
        /// </summary>
        /// <param name="timePeriods">The time periods to check for overlapping instances.</param>
        /// <returns>A collection of time periods that overlap the current instance.</returns>
        public IEnumerable<TimePeriod> GetOverlappingTimePeriods(IEnumerable<TimePeriod> timePeriods)
        {
            Guard.NotNull(nameof(timePeriods), timePeriods);

            return timePeriods.Where(Overlaps);
        }

        /// <summary>
        /// Checks that the given time period isn't overlapped by the current instance.
        /// </summary>
        /// <param name="timePeriod">The time period that is to be checked to determine whether or not it overlaps this one.</param>
        /// <returns>True if the given time period is overlapped.</returns>
        public bool Overlaps(TimePeriod timePeriod)
        {
            Guard.NotNull(nameof(timePeriod), timePeriod);

            return (EndDate > timePeriod.StartDate && EndDate < timePeriod.EndDate)
                   || (StartDate > timePeriod.StartDate && StartDate < timePeriod.EndDate)
                   || (StartDate < timePeriod.StartDate && EndDate > timePeriod.EndDate)
                   || (EndDate == timePeriod.EndDate)
                   || (StartDate == timePeriod.StartDate);
        }
    }
}