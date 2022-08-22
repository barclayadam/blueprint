﻿using System;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.IsInFutureAttribute_Tests;

public class Given_Date
{
    [Test]
    public void When_DateTime_Is_In_Future_Then_Valid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);

        // Act
        var isValid = inFutureAttribute.IsValid(DateTime.UtcNow.AddDays(1));

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void When_DateTime_Is_In_Past_Then_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);

        // Act
        var isValid = inFutureAttribute.IsValid(DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void When_DateTime_Is_Invalid_String_Then_Is_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);
        var dateTime = "Tenth Of June 2011";

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void When_DateTime_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);
        const int dateInteger = 10102010;

        // Act
        var exception = Assert.Catch<InvalidOperationException>(() => inFutureAttribute.IsValid(dateInteger));

        // Assert
        exception.Should().NotBeNull();
    }

    [Test]
    public void When_DateTime_Is_String_In_Future_Then_Valid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);
        var dateTime = DateTime.Now.AddDays(1).Date.ToString();

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void When_DateTime_Is_String_In_Past_Then_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.DateTime);
        var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void When_Date_Is_In_Future_Then_Valid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);

        // Act
        var isValid = inFutureAttribute.IsValid(DateTime.UtcNow.AddDays(1));

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void When_DateTimeOffset_Is_In_Future_Then_Valid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);

        // Act
        var isValid = inFutureAttribute.IsValid(DateTimeOffset.Now.AddDays(1));

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void When_Date_Is_In_Past_Then_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);

        // Act
        var isValid = inFutureAttribute.IsValid(DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void When_Date_Is_Invalid_String_Then_Is_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);
        var dateTime = "Tenth Of June 2011";

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void When_Date_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);
        const int dateInteger = 10102010;

        // Act
        var exception = Assert.Catch<InvalidOperationException>(() => inFutureAttribute.IsValid(dateInteger));

        // Assert
        exception.Should().NotBeNull();
    }

    [Test]
    public void When_Date_Is_String_In_Future_Then_Valid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);
        var dateTime = DateTime.Now.AddDays(1).Date.ToString();

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void When_Date_Is_String_In_Past_Then_Invalid()
    {
        // Arrange
        var inFutureAttribute = new InFutureAttribute(TemporalCheck.Date);
        var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

        // Act
        var isValid = inFutureAttribute.IsValid(dateTime);

        // Assert
        Assert.IsFalse(isValid);
    }
}