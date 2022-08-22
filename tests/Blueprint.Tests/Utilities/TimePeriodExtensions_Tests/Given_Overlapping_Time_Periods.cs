using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.TimePeriodExtensions_Tests;

public class Given_Overlapping_Time_Periods
{
    [Test]
    public void When_TimePeriod_Ends_At_Same_Time_As_TimePeriod_And_Starts_After_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Ends_At_Same_Time_As_TimePeriod_And_Starts_Before_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Is_The_Same_As_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_End_Of_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_End_Of_TimePeriod_Then_True_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));

        // Act
        var periodOverlaps = period.Overlaps(testPeriod);

        // Assert
        periodOverlaps.Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_Start_Of_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_Start_Of_TimePeriod_Then_True_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));

        // Act
        var periodOverlaps = period.Overlaps(testPeriod);

        // Assert
        periodOverlaps.Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_The_Whole_Of_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Overlaps_The_Whole_Of_TimePeriod_Then_True_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

        // Act
        var periodOverlaps = period.Overlaps(testPeriod);

        // Assert
        periodOverlaps.Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Starts_At_Same_Time_As_TimePeriod_And_Ends_After_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }

    [Test]
    public void When_TimePeriod_Starts_At_Same_Time_As_TimePeriod_And_Ends_Before_TimePeriod_Then_TimePeriod_Is_Marked_As_Overlapping()
    {
        // Arrange
        var period = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(2));
        var testPeriod = new TimePeriod(DateTime.Today, DateTime.Today.AddDays(3));
        var periodCollection = new List<TimePeriod> { testPeriod };

        // Act
        var overlappingPeriods = period.GetOverlappingTimePeriods(periodCollection);

        // Assert
        overlappingPeriods.Contains(testPeriod).Should().BeTrue();
    }
}