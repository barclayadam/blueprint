using System;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MaxItemsListAttribute_Tests;

public class Given_Non_Enumerable_Value
{
    [Test]
    public void When_Object_Is_Not_Enumerable_Then_Exception_Is_Thrown()
    {
        // Arrange
        var inFutureAttribute = new MaxItemsListAttribute(10);
        var notList = new object();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => inFutureAttribute.IsValid(notList));

        // Assert
        exception.Should().NotBeNull();
    }
}