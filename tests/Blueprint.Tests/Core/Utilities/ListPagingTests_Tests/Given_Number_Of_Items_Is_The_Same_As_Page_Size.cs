using System;
using System.Linq;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.ListPagingTests_Tests
{
    public class Given_Number_Of_Items_Is_The_Same_As_Page_Size
    {
        [Test]
        public void When_I_Get_A_Page_Requesting_Page_That_Does_Not_Exist_Then_InvalidOperationException_Is_Thrown()
        {
            // Arrange
            var list = new object[10];

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => list.ToPage(2, 10));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        [TestCase(1)]
        [TestCase(10)]
        public void When_I_Get_A_Page_Then_Number_Of_Items_In_List_Stays_The_Same(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, numberOfItems);

            // Assert
            result.Items.Count().Should().Be(numberOfItems);
        }

        [Test]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(10)]
        public void When_I_Get_A_Page_Then_PageCount_Is_One(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, numberOfItems);

            // Assert
            result.PageCount.Should().Be(1);
        }

        [Test]
        public void When_I_Get_A_Page_Then_PagedList_PageNumber_Stays_The_Same()
        {
            // Arrange
            var list = new object[1];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.PageNumber.Should().Be(1);
        }

        [Test]
        [TestCase(10)]
        [TestCase(20)]
        public void When_I_Get_A_Page_Then_The_PageSize_Stays_The_Same(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, numberOfItems);

            // Assert
            result.PageSize.Should().Be(numberOfItems);
        }

        [Test]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void When_I_Get_A_Page_Then_TotalCount_Is_The_Size_Of_The_Collection(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, numberOfItems);

            // Assert
            result.TotalCount.Should().Be(numberOfItems);
        }
    }
}
