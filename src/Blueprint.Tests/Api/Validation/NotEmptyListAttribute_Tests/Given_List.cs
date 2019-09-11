using Blueprint.Api.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Validation.NotEmptyListAttribute_Tests
{
    public class Given_List
    {
        [Test]
        public void When_List_Is_Empty_Then_Invalid()
        {
            // Arrange
            var notEmptyListAttribute = new NotEmptyListAttribute();
            var emptyList = new object[0];

            // Act
            var isValid = notEmptyListAttribute.IsValid(emptyList);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_List_Is_Populated_Then_Valid()
        {
            // Arrange
            var notEmptyListAttribute = new NotEmptyListAttribute();
            var populatedList = new object[] { 1 };

            // Act
            var isValid = notEmptyListAttribute.IsValid(populatedList);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}