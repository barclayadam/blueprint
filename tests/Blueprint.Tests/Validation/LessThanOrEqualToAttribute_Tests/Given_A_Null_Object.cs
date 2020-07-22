using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.LessThanOrEqualToAttribute_Tests
{
    public class Given_A_Null_Object
    {
        [Test]
        public void When_Object_Is_Null_Then_Valid()
        {
            // Arrange
            var lessThanAttribute = new LessThanOrEqualToAttribute(1);
            object nullObject = null;

            // Act
            var isValid = lessThanAttribute.IsValid(nullObject);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
