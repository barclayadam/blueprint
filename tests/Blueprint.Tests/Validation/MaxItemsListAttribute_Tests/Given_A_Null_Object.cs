using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MaxItemsListAttribute_Tests
{
    public class Given_A_Null_Object
    {
        [Test]
        public void When_Object_Is_Null_Then_Valid()
        {
            // Arrange
            var maxItemsListAttribute = new MaxItemsListAttribute(1);
            object nullObject = null;

            // Act
            var isValid = maxItemsListAttribute.IsValid(nullObject);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
