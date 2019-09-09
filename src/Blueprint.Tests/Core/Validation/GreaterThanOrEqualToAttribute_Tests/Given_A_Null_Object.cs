using Blueprint.Core.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Validation.GreaterThanOrEqualToAttribute_Tests
{
    public class Given_A_Null_Object
    {
        [Test]
        public void When_Object_Is_Null_Then_Valid()
        {
            // Arrange
            var greaterThanAttribute = new GreaterThanOrEqualToAttribute(1);
            object nullObject = null;

            // Act
            var isValid = greaterThanAttribute.IsValid(nullObject);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}