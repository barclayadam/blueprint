using System;
using Blueprint.Core;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Authorisation.ResourceKey_Tests
{
    public class Given_Invalid_Int_Resource_Key_String
    {
        [Test]
        public void When_Key_Contains_Invalid_Guid_Parse_Then_FormatException_Is_Thrown()
        {
            // Arrange
            var resourceKeyString = "Site/sdfrerf";

            // Act
            var exception = Assert.Throws<FormatException>(() => ResourceKey<int>.Parse(resourceKeyString));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Key_Contains_Invalid_Guid_TryParse_Then_False_Is_Retuend()
        {
            // Arrange
            ResourceKey<int> resourceKey;

            // Act
            var resourceKeyIsValid = ResourceKey<int>.TryParse("Site/dsfswefr fwrf", out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeFalse();
        }

        [Test]
        public void When_Key_Does_Not_Contain_Slash_Parse_Then_FormatException_Is_Thrown()
        {
            // Arrange
            var resourceKeyString = "Site54581";

            // Act
            var exception = Assert.Throws<FormatException>(() => ResourceKey<int>.Parse(resourceKeyString));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Key_Does_Not_Contain_Slash_TryParse_Then_False_Is_Returned()
        {
            // Arrange
            ResourceKey<int> resourceKey;
            var resourceKeyString = "Site84112";

            // Act
            var resourceKeyIsValid = ResourceKey<int>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeFalse();
        }

        [Test]
        public void When_Key_Is_Empty_String_Parse_Then_Null_Is_Returned()
        {
            // Arrange
            var resourceKeyString = string.Empty;

            // Act
            var resourceKey = ResourceKey<int>.Parse(resourceKeyString);

            // Assert
            resourceKey.Should().BeNull();
        }

        [Test]
        public void When_Key_Is_Empty_String_TryParse_Then_True_Is_Retuend()
        {
            // Arrange
            ResourceKey<int> resourceKey;
            var resourceKeyString = string.Empty;

            // Act
            var resourceKeyIsValid = ResourceKey<int>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }

        [Test]
        public void When_Key_Is_Null_Parse_Then_Null_Is_Returned()
        {
            // Arrange
            string resourceKeyString = null;

            // Act
            var resourceKey = ResourceKey<int>.Parse(resourceKeyString);

            // Assert
            resourceKey.Should().BeNull();
        }

        [Test]
        public void When_Key_Is_Null_TryParse_Then_True_Is_Retuend()
        {
            // Arrange
            ResourceKey<int> resourceKey;
            string resourceKeyString = null;

            // Act
            var resourceKeyIsValid = ResourceKey<int>.TryParse(resourceKeyString, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }
    }
}