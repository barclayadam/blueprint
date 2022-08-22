using System;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.LessThanOrEqualToAttribute_Tests;

public class Given_Numeric_Value
{
    [Test]
    public void When_Value_Is_Equal_To_Required_Then_Valid()
    {
        // Arrange
        var lessThanAttribute = new LessThanOrEqualToAttribute(2);
        var value = 2;

        // Act
        var isValid = lessThanAttribute.IsValid(value);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void When_Value_Is_Greater_Than_Required_Then_Invalid()
    {
        // Arrange
        var lessThanAttribute = new LessThanOrEqualToAttribute(2);
        var value = 4;

        // Act
        var isValid = lessThanAttribute.IsValid(value);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void When_Value_Is_Less_Than_Required_Value_Then_Valid()
    {
        // Arrange
        var lessThanAttribute = new LessThanOrEqualToAttribute(2);
        var value = 0;

        // Act
        var isValid = lessThanAttribute.IsValid(value);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    [TestCase(typeof(byte), 9)]
    [TestCase(typeof(sbyte), 9)]
    [TestCase(typeof(short), 9)]
    [TestCase(typeof(ushort), 9)]
    [TestCase(typeof(int), 9)]
    [TestCase(typeof(uint), 9)]
    [TestCase(typeof(long), 9)]
    [TestCase(typeof(ulong), 9)]
    [TestCase(typeof(string), "9")]
    [TestCase(typeof(float), 9.1)]
    [TestCase(typeof(double), 9.1)]
    [TestCase(typeof(decimal), 9.1)]
    [TestCase(typeof(string), "9.1")]
    public void When_Value_Type_Is_Numeric_And_Less_Than_Required_Then_Valid(Type type, object value)
    {
        // Arrange
        var lessThanAttribute = new LessThanOrEqualToAttribute(10);
        var typedValue = Convert.ChangeType(value, type);

        // Act
        var isValid = lessThanAttribute.IsValid(typedValue);

        // Assert
        isValid.Should().BeTrue();
    }
}