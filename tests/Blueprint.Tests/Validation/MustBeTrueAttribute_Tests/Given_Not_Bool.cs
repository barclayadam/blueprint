using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MustBeTrueAttribute_Tests;

public class Given_Not_Bool
{
    [TestCase]
    public void When_String_Then_Invalid()
    {
        // ARRANGE
        var attribute = new MustBeTrueAttribute();

        // ACT
        var isValid = attribute.IsValid("Hello");

        // ASSERT
        isValid.Should().BeFalse();
    }

    [TestCase]
    public void When_Integer_Then_Invalid()
    {
        // ARRANGE
        var attribute = new MustBeTrueAttribute();

        // ACT
        var isValid = attribute.IsValid(1);

        // ASSERT
        isValid.Should().BeFalse();
    }
}