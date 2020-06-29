using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Authorisation.ResourceKeyAttribute_Tests
{
    public class Given_ResourceKey
    {
        [Test]
        [TestCase("Site/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("SiteGroup/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("Customer/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("User/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("Something/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("Anything/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("SomethingElseVeryLong0123456789/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        public void When_ResourceKey_Contains_Correct_Values_Then_Valid(string resourceKey)
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid(resourceKey);

            // Assert
            isValid.Should().BeTrue();
        }

        [Test]
        public void When_ResourceKey_Contains_Invalid_Guid_Then_Invalid()
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid("SiteGroup/NOTAGUID");

            // Assert
            isValid.Should().BeFalse();
        }

        [Test]
        public void When_ResourceKey_Does_Not_Contain_Guid_Then_Invalid()
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid("SiteGroup/");

            // Assert
            isValid.Should().BeFalse();
        }

        [Test]
        public void When_ResourceKey_Does_Not_Contain_Type_Then_Invalid()
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid("/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3");

            // Assert
            isValid.Should().BeFalse();
        }

        [Test]
        public void When_ResourceKey_Does_Not_Slash_Type_Then_Invalid()
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid("SiteGroupE0300EEE-1199-48EE-8B55-06F1EBF7B4C3");

            // Assert
            isValid.Should().BeFalse();
        }

        [Test]
        [TestCase("5Type/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        [TestCase("+Type/E0300EEE-1199-48EE-8B55-06F1EBF7B4C3")]
        public void When_ResourceKey_Type_Does_Not_Start_With_Letter_Then_Invalid(string resourceKey)
        {
            // Arrange
            var resourceKeyAttribute = new ResourceKeyAttribute();

            // Act
            var isValid = resourceKeyAttribute.IsValid(resourceKey);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
