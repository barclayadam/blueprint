using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.MaxItemsListAttribute_Tests
{
    public class Given_List
    {
        [Test]
        public void When_List_Contains_Fewer_Items_Than_Required_Then_Valid()
        {
            // Arrange
            var maxItemsListAttribute = new MaxItemsListAttribute(2);
            var populatedList = new object[] { 1 };

            // Act
            var isValid = maxItemsListAttribute.IsValid(populatedList);

            // Assert
            isValid.Should().BeTrue();
        }

        [Test]
        public void When_List_Contains_More_Items_Than_Required_Then_Invalid()
        {
            // Arrange
            var maxItemsListAttribute = new MaxItemsListAttribute(2);
            var populatedList = new object[] { 1, 2, 3 };

            // Act
            var isValid = maxItemsListAttribute.IsValid(populatedList);

            // Assert
            isValid.Should().BeFalse();
        }

        [Test]
        public void When_List_Contains_Number_Of_Items_Required_Then_Valid()
        {
            // Arrange
            var maxItemsListAttribute = new MaxItemsListAttribute(2);
            var populatedList = new object[] { 1, 2 };

            // Act
            var isValid = maxItemsListAttribute.IsValid(populatedList);

            // Assert
            isValid.Should().BeTrue();
        }

        [Test]
        public void When_List_Is_Empty_Then_Valid()
        {
            // Arrange
            var maxItemsListAttribute = new MaxItemsListAttribute(2);
            var emptyList = new object[0];

            // Act
            var isValid = maxItemsListAttribute.IsValid(emptyList);

            // Assert
            isValid.Should().BeTrue();
        }
    }
}
