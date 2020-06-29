using System;
using System.Linq;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.ListPagingTests_Tests
{
    public class Given_Fewer_Items_Than_Page_Size
    {
        [Test]
        public void When_I_Get_A_Page_Requesting_Page_That_Does_Not_Exist_Then_InvalidOperationException_Is_Thrown()
        {
            // Arrange
            var list = new object[5];

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => list.ToPage(2, 10));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        [TestCase(1)]
        [TestCase(10)]
        public void When_I_Get_A_Page_Then_Count_Of_Items_Is_Same_As_Input(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, numberOfItems + 1);

            // Assert
            result.Items.Count().Should().Be(numberOfItems);
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(10, 13)]
        [TestCase(10, 15)]
        public void When_I_Get_A_Page_Then_PageCount_Is_One(int numberOfItems, int pageSize)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, pageSize);

            // Assert
            result.PageCount.Should().Be(1);
        }

        [Test]
        public void When_I_Get_A_Page_Then_PagedList_PageNumber_Stays_The_Same()
        {
            // Arrange
            var list = new object[1];

            // Act
            var result = list.ToPage(1, 2);

            // Assert
            result.PageNumber.Should().Be(1);
        }

        [Test]
        public void When_I_Get_A_Page_Then_Returned_Items_Are_The_Given_Items()
        {
            // Arrange
            var list = new[] { 1, 2, 3 };

            // Act
            var result = list.ToPage(1, 5);

            // Assert
            result.Items.ElementAt(0).Should().Be(list[0]);
            result.Items.ElementAt(1).Should().Be(list[1]);
            result.Items.ElementAt(2).Should().Be(list[2]);
        }

        [Test]
        [TestCase(10)]
        [TestCase(20)]
        public void When_I_Get_A_Page_Then_The_PageSize_Stays_The_Same(int pageSize)
        {
            // Arrange
            var list = new object[pageSize - 1];

            // Act
            var result = list.ToPage(1, pageSize);

            // Assert
            result.PageSize.Should().Be(pageSize);
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
            var result = list.ToPage(1, numberOfItems + 1);

            // Assert
            result.TotalCount.Should().Be(numberOfItems);
        }
    }
}
