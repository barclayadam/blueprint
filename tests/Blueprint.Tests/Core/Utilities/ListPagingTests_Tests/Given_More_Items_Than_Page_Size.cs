using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Core.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.ListPagingTests_Tests
{
    public class Given_More_Items_Than_Page_Size
    {
        [Test]
        public void When_I_Convert_To_A_Non_First_Page_Then_That_Page_Contains_The_Items_From_The_Input()
        {
            // Arrange
            var list = new[] { 1, 2, 3 };

            // Act
            var result = list.ToPage(2, 2);

            // Assert
            result.Items.ElementAt(0).Should().Be(list[2]);
        }

        [Test]
        public void When_I_Convert_To_The_First_Page_Then_That_Page_Contains_The_Items_From_The_Input()
        {
            // Arrange
            var list = new[] { 1, 2, 3 };

            // Act
            var result = list.ToPage(1, 2);

            // Assert
            result.Items.ElementAt(0).Should().Be(list[0]);
            result.Items.ElementAt(1).Should().Be(list[1]);
        }

        [Test]
        [TestCase(11, 1)]
        [TestCase(15, 1)]
        [TestCase(150, 5)]
        public void When_I_Get_A_Page_Requesting_Full_Page_Then_Count_Of_Items_In_List_Is_Page_Size(
                int numberOfItems, int page)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(page, 10);

            // Assert
            result.Items.Count().Should().Be(10);
        }

        [Test]
        public void When_I_Get_A_Page_Requesting_Page_That_Does_Not_Exist_Then_InvalidOperationException_Is_Thrown()
        {
            // Arrange
            var list = new object[20];

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => list.ToPage(3, 10));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        [TestCaseSource(nameof(ElevenToTwenty))]
        public void When_I_Get_A_Page_Then_Last_Page_Number_Of_Items_In_List_Is_Remaining_Items(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(2, 10);

            // Assert
            result.Items.Count().Should().Be(numberOfItems - 10);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void When_I_Get_A_Page_Then_PagedList_PageNumber_Stays_The_Same(int pageNumber)
        {
            // Arrange
            var list = new object[20];

            // Act
            var result = list.ToPage(pageNumber, 10);

            // Assert
            result.PageNumber.Should().Be(pageNumber);
        }

        [Test]
        [TestCase(10)]
        [TestCase(20)]
        public void When_I_Get_A_Page_Then_The_PageSize_Stays_The_Same(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.PageSize.Should().Be(1);
        }

        [Test]
        [TestCase(10)]
        [TestCase(100)]
        public void When_I_Get_A_Page_Then_TotalCount_Is_The_Size_Of_The_Collection(int numberOfItems)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.TotalCount.Should().Be(numberOfItems);
        }

        [Test]
        [TestCase(11, 2)]
        [TestCase(20, 2)]
        [TestCase(25, 3)]
        public void When_I_Get_A_Page_With_PageSize_Ten_Then_PageCount_Is_As_Expected(
                int numberOfItems, int expectedPageCount)
        {
            // Arrange
            var list = new object[numberOfItems];

            // Act
            var result = list.ToPage(1, 10);

            // Assert
            result.PageCount.Should().Be(expectedPageCount);
        }

        private static IEnumerable<int> ElevenToTwenty()
        {
            return Enumerable.Range(11, 9);
        }
    }
}