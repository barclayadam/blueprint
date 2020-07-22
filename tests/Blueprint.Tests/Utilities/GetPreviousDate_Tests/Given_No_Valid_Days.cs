using System;
using System.Collections.Generic;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.GetPreviousDate_Tests
{
    public class Given_No_Valid_Days
    {
        [Test]
        public void When_No_Days_Provided_Then_ArgumentException_Is_Thrown()
        {
            // Arrange
            var validDays = new List<DayOfWeek>();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => DateTime.Now.GetPreviousDate(validDays));

            // Assert
            exception.Should().NotBeNull();
        }
    }
}
