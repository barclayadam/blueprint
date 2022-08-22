using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MustBeTrueAttribute_Tests;

public class Given_Bool
{
    [Test]
    public void When_False_Then_Invalid()
    {
        // ARRANGE
        var attribute = new MustBeTrueAttribute();

        // ACT
        var isValid = attribute.IsValid(false);

        // ASSERT
        isValid.Should().BeFalse();
    }

    [Test]
    public void When_True_Then_Valid()
    {
        // ARRANGE
        var attribute = new MustBeTrueAttribute();

        // ACT
        var isValid = attribute.IsValid(true);

        // ASSERT
        isValid.Should().BeTrue();
    }
}