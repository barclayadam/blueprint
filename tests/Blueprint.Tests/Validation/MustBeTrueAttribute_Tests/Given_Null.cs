using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MustBeTrueAttribute_Tests
{
    public class Given_Null
    {
        [TestCase]
        public void When_Null_Then_Valid()
        {
            // ARRANGE
            var attribute = new MustBeTrueAttribute();

            // ACT
            var result = attribute.IsValid(null);

            // ASSERT
            result.Should().BeTrue();
        }
    }
}
