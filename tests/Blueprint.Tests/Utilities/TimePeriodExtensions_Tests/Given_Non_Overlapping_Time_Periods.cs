using System;
using System.Collections.Generic;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.TimePeriodExtensions_Tests;

public class Given_Non_Overlapping_Time_Periods
{
    [Test]
    public void When_Checking_Overlapping_TimePeriods_With_Period_After_Then_False_Is_Returned()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));

        // Act
        var periodOverlaps = period.Overlaps(testPeriod);

        // Assert
        periodOverlaps.Should().BeFalse();
    }

    [Test]
    public void When_Checking_Overlapping_TimePeriods_With_Period_Before_Then_False_Is_Returned()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));

        // Act
        var periodOverlaps = period.Overlaps(testPeriod);

        // Assert
        periodOverlaps.Should().BeFalse();
    }

    [Test]
    public void When_Getting_Overlapping_TimePeriods_With_Period_After_Then_No_TimePeriods_Are_Returned()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Should().BeEmpty();
    }

    [Test]
    public void When_Getting_Overlapping_TimePeriods_With_Period_Before_Then_No_TimePeriods_Are_Returned()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Should().BeEmpty();
    }
}