using Blueprint.Core.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.ListPagingTests_Tests
{
    public class Given_An_Empty_List
    {
        [Test]
        public void When_I_Get_A_Page_Then_PageCount_Is_Zero()
        {
            // Arrange
            var list = new object[0];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.PageCount.Should().Be(0);
        }

        [Test]
        public void When_I_Get_A_Page_Then_PagedList_PageNumber_Is_Zero()
        {
            // Arrange
            var list = new object[0];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.PageNumber.Should().Be(0);
        }

        [Test]
        public void When_I_Get_A_Page_Then_PagedList_With_Empty_Result_Set_Returned()
        {
            // Arrange
            var list = new object[0];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.Items.Should().BeEmpty();
        }

        [Test]
        [TestCase(1)]
        [TestCase(int.MaxValue)]
        public void When_I_Get_A_Page_Then_The_PageSize_Stays_The_Same(int pageSize)
        {
            // Arrange
            var list = new object[0];

            // Act
            var result = list.ToPage(1, pageSize);

            // Assert
            result.PageSize.Should().Be(pageSize);
        }

        [Test]
        public void When_I_Get_A_Page_Then_TotalCount_Is_Zero()
        {
            // Arrange
            var list = new object[0];

            // Act
            var result = list.ToPage(1, 1);

            // Assert
            result.TotalCount.Should().Be(0);
        }
    }
}