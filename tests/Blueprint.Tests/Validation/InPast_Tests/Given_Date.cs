﻿using System;
using Blueprint.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.InPast_Tests;

public class Given_Date
{
    [Test]
    public void When_DateTime_Is_Max_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.MaxValue;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Min_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.MinValue;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Now_Minus_One_Second_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Now_Plus_One_Second_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddSeconds(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Now_Then_Invalid()
    {
        using var t = SystemTime.PauseForThread();

        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_DateTime_Is_The_Current_Time_Tomorrow_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_DateTime_Is_The_Current_Time_Yesterday_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Today_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Tomorrow_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_DateTime_Is_Yesterday_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date.AddDays(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.DateTime);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_Date_Is_Max_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.MaxValue;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_Min_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.MinValue;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_Date_Is_Now_Minus_One_Second_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_Now_Plus_One_Second_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddSeconds(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_Now_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_The_Current_Time_Tomorrow_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddDays(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_The_Current_Time_Yesterday_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsTrue(isInPast);
    }

    [Test]
    public void When_Date_Is_Today_Then_Invalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date;

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_Tomorrow_Then_Inalid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsFalse(isInPast);
    }

    [Test]
    public void When_Date_Is_Yesterday_Then_Valid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.Date.AddDays(-1);

        // Act
        var isInPast = dateTime.IsInPast(TemporalCheck.Date);

        // Assert
        Assert.IsTrue(isInPast);
    }
}