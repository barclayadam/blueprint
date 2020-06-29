using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Validation.NotEmptyListAttribute_Tests
{
    public class Given_A_Null_Object
    {
        [Test]
        public void When_Object_Is_Null_Then_Valid()
        {
            // Arrange
            var notEmptyListAttribute = new NotEmptyListAttribute();
            object nullObject = null;

            // Act
            var isValid = notEmptyListAttribute.IsValid(nullObject);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
