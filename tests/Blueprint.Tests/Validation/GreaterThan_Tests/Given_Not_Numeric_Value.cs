using System;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.GreaterThan_Tests
{
    public class Given_Not_Numeric_Value
    {
        [Test]
        public void When_Value_Type_Is_Not_Numeric_Then_Exception_Is_Thrown()
        {
            // Arrange
            var greaterThanAttribute = new GreaterThanAttribute(10);
            const string typedValue = "A String";

            // Act
            var exception = Assert.Throws<FormatException>(() => greaterThanAttribute.IsValid(typedValue));

            // Assert
            exception.Should().NotBeNull();
        }
    }
}
