using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Authorisation.ResourceKey_Tests
{
    public class Given_Valid_Int_Resource_Key_String
    {
        [Test]
        public void When_Checking_Validitiy_Of_Resource_Key_Then_Key_Is_Valid()
        {
            // Arrange
            ResourceKey<int> resourceKey;

            // Act
            var resourceKeyIsValid = ResourceKey<int>.TryParse("Site/0", out resourceKey);

            // Assert
            resourceKeyIsValid.Should().BeTrue();
        }

        [Test]
        public void When_Retrieving_Resource_Key_Id_Then_Correct_Id_Is_Returned()
        {
            // Arrange
            const int resourceKeyId = 1234;
            var resourceKey = ResourceKey<int>.Parse("Site/" + resourceKeyId);

            // Act
            var returnedResourceKeyId = resourceKey.Id;

            // Assert
            returnedResourceKeyId.Should().Be(resourceKeyId);
        }

        [Test]
        public void When_Retrieving_Resource_Key_ToString_Then_It_Is_Returned_Unchanged()
        {
            // Arrange
            const string resourceKeyString = "Site/1234";
            var resourceKey = ResourceKey<int>.Parse(resourceKeyString);

            // Act
            var returnedResourceKey = resourceKey.ToString();

            // Assert
            resourceKeyString.Should().Be(returnedResourceKey);
        }

        [Test]
        public void When_Retrieving_Resource_Key_Type_Then_Correct_Type_Is_Returned()
        {
            // Arrange
            const string resourceKeyType = "Site";
            const int resourceKeyId = 1234;
            var resourceKey = ResourceKey<int>.Parse(resourceKeyType + "/" + resourceKeyId);

            // Act
            var returnedResourceKeyType = resourceKey.ResourceType;

            // Assert
            returnedResourceKeyType.Should().Be(resourceKeyType);
        }
    }
}
