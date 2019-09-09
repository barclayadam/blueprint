using System;
using System.Collections.Generic;
using Blueprint.Core.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.GetPreviousDate_Tests
{
    public class Given_A_Monday
    {
        [Test]
        public void When_All_Days_Are_Valid_Then_Previous_Day_Is_Sunday()
        {
            // Arrange
            var validDays = new List<DayOfWeek>
                            {
                                DayOfWeek.Monday,
                                DayOfWeek.Tuesday,
                                DayOfWeek.Wednesday,
                                DayOfWeek.Thursday,
                                DayOfWeek.Friday,
                                DayOfWeek.Saturday,
                                DayOfWeek.Sunday
                            };
            var date = GetDay(DayOfWeek.Monday);

            // Act
            var previousDate = date.GetPreviousDate(validDays);

            // Assert
            previousDate.DayOfWeek.Should().Be(DayOfWeek.Sunday);
            previousDate.Date.Should().Be(date.AddDays(-1).Date);
        }

        [Test]
        public void When_Mondays_Are_Invalid_Then_Previous_Day_Is_Sunday()
        {
            // Arrange
            var validDays = new List<DayOfWeek>
                            {
                                DayOfWeek.Tuesday,
                                DayOfWeek.Wednesday,
                                DayOfWeek.Thursday,
                                DayOfWeek.Friday,
                                DayOfWeek.Saturday,
                                DayOfWeek.Sunday
                            };
            var date = GetDay(DayOfWeek.Monday);

            // Act
            var previousDate = date.GetPreviousDate(validDays);

            // Assert
            previousDate.DayOfWeek.Should().Be(DayOfWeek.Sunday);
            previousDate.Date.Should().Be(date.AddDays(-1).Date);
        }

        [Test]
        public void When_Only_Mondays_Are_Valid_Then_Previous_Day_Is_Monday()
        {
            // Arrange
            var validDays = new List<DayOfWeek>
                            {
                                DayOfWeek.Monday
                            };
            var date = GetDay(DayOfWeek.Monday);

            // Act
            var previousDate = date.GetPreviousDate(validDays);

            // Assert
            previousDate.DayOfWeek.Should().Be(DayOfWeek.Monday);
            previousDate.Date.Should().Be(date.AddDays(-7).Date);
        }

        [Test]
        public void When_Only_Week_Days_Are_Valid_Then_Previous_Day_Is_Friday()
        {
            // Arrange
            var validDays = new List<DayOfWeek>
                            {
                                DayOfWeek.Monday,
                                DayOfWeek.Tuesday,
                                DayOfWeek.Wednesday,
                                DayOfWeek.Thursday,
                                DayOfWeek.Friday
                            };
            var date = GetDay(DayOfWeek.Monday);

            // Act
            var previousDate = date.GetPreviousDate(validDays);

            // Assert
            previousDate.DayOfWeek.Should().Be(DayOfWeek.Friday);
            previousDate.Date.Should().Be(date.AddDays(-3).Date);
        }

        private static DateTime GetDay(DayOfWeek day)
        {
            var date = DateTime.Now;

            while (date.DayOfWeek != day)
            {
                date = date.AddDays(-1);
            }

            return date;
        }
    }
}
