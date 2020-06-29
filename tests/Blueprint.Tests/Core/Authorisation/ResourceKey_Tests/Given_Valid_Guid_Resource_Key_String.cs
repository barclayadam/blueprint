using System;
using Blueprint;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Authorisation.ResourceKey_Tests
{
    public class Given_Valid_Guid_Resource_Key_String
    {
        [Test]
        public void When_Checking_Validitiy_Of_Resource_Key_Then_Key_Is_Valid()
        {
            // Arrange
            ResourceKey<Guid> resourceKey;

            // Act
            var resourceKeyIsValid = ResourceKey<Guid>.TryParse("Site/" + Guid.Empty, out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }

        [Test]
        public void When_Retrieving_Resource_Key_Id_Then_Correct_Id_Is_Returned()
        {
            // Arrange
            var resourceKeyId = Guid.NewGuid();
            var resourceKey = ResourceKey<Guid>.Parse("Site/" + resourceKeyId);

            // Act
            var returnedResourceKeyId = resourceKey.Id;

            // Assert
            returnedResourceKeyId.Should().Be(resourceKeyId);
        }

        [Test]
        public void When_Retrieving_Resource_Key_ToString_Then_It_Is_Returned_Unchanged()
        {
            // Arrange
            var resourceKeyString = "Site/" + Guid.Empty;
            var resourceKey = ResourceKey<Guid>.Parse(resourceKeyString);

            // Act
            var returnedResourceKey = resourceKey.ToString();

            // Assert
            resourceKeyString.Should().Be(returnedResourceKey);
        }

        [Test]
        public void When_Retrieving_Resource_Key_Type_Then_Correct_Type_Is_Returned()
        {
            // Arrange
            const string ResourceKeyType = "Site";
            var resourceKey = ResourceKey<Guid>.Parse(ResourceKeyType + "/" + Guid.Empty);

            // Act
            var returnedResourceKeyType = resourceKey.ResourceType;

            // Assert
            returnedResourceKeyType.Should().Be(ResourceKeyType);
        }
    }
}
