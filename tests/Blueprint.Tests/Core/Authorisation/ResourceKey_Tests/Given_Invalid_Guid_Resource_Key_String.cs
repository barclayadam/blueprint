using System;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Authorisation.ResourceKey_Tests
{
    public class Given_Invalid_Guid_Resource_Key_String
    {
        [Test]
        public void When_Key_Contains_Invalid_Guid_Parse_Then_FormatException_Is_Thrown()
        {
            // Arrange
            var resourceKeyString = "Site/8493028409238409890238409238049";

            // Act
            var exception = Assert.Throws<FormatException>(() => ResourceKey<Guid>.Parse(resourceKeyString));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Key_Contains_Invalid_Guid_TryParse_Then_False_Is_Retuend()
        {
            // Arrange
            ResourceKey<Guid> resourceKey;

            // Act
            var resourceKeyIsValid = ResourceKey<Guid>.TryParse("Site/8493028409238409890238409238049", out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeFalse();
        }

        [Test]
        public void When_Key_Does_Not_Contain_Slash_Parse_Then_FormatException_Is_Thrown()
        {
            // Arrange
            var resourceKeyString = "Site" + Guid.NewGuid();

            // Act
            var exception = Assert.Throws<FormatException>(() => ResourceKey<Guid>.Parse(resourceKeyString));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Key_Does_Not_Contain_Slash_TryParse_Then_False_Is_Retuend()
        {
            // Arrange
            ResourceKey<Guid> resourceKey;
            var resourceKeyString = "Site" + Guid.NewGuid();

            // Act
            var resourceKeyIsValid = ResourceKey<Guid>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeFalse();
        }

        [Test]
        public void When_Key_Is_Empty_String_Parse_Then_Null_Is_Returned()
        {
            // Arrange
            var resourceKeyString = string.Empty;

            // Act
            var resourceKey = ResourceKey<Guid>.Parse(resourceKeyString);

            // Assert
            resourceKey.Should().BeNull();
        }

        [Test]
        public void When_Key_Is_Empty_String_TryParse_Then_True_Is_Retuend()
        {
            // Arrange
            ResourceKey<Guid> resourceKey;
            var resourceKeyString = string.Empty;

            // Act
            var resourceKeyIsValid = ResourceKey<Guid>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }

        [Test]
        public void When_Key_Is_Null_Parse_Then_Null_Is_Returned()
        {
            // Arrange
            string resourceKeyString = null;

            // Act
            var resourceKey = ResourceKey<Guid>.Parse(resourceKeyString);

            // Assert
            resourceKey.Should().BeNull();
        }

        [Test]
        public void When_Key_Is_Null_TryParse_Then_True_Is_Retuend()
        {
            // Arrange
            ResourceKey<Guid> resourceKey;
            string resourceKeyString = null;

            // Act
            var resourceKeyIsValid = ResourceKey<Guid>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }
    }
}
