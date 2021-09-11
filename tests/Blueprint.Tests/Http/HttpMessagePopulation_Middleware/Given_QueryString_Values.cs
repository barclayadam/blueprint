using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware
{
    public class Given_QueryString_Values
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [RootLink("/query-values")]
        public class QueryTestOperation
        {
            public int IntegerProperty { get; set; }

            public int? NullableIntegerProperty { get; set; }

            public string StringProperty { get; set; }

            public OperationEnum EnumProperty { get; set; }

            public Guid GuidProperty { get; set; }

            public Length Length { get; set; }

            public Length[] Lengths { get; set; }

            public IEnumerable<string> StringEnumerable { get; set; }

            public string[] StringArray { get; set; }

            public List<string> StringList { get; set; }

            public string AReadOnlyProperty => "Something read only";
        }

        [RootLink("/query-multi-values")]
        public class QueryMultiValueOperation<T>
        {
            public IEnumerable<T> Enumerable { get; set; }

            public T[] Array { get; set; }

            public List<T> List { get; set; }
        }

        [Test]
        public async Task When_Simple_Properties_Then_Populated()
        {
            // Arrange
            var expected = new QueryTestOperation
            {
                IntegerProperty = 761,
                EnumProperty = OperationEnum.EnumOne,
                GuidProperty = Guid.NewGuid(),
                StringProperty = "a string",
                NullableIntegerProperty = null
            };

            // Act / Assert
            await AssertPopulatedFromQueryString(
                expected,
                $"?{nameof(expected.IntegerProperty)}={expected.IntegerProperty}&" +
                $"{nameof(expected.EnumProperty)}={expected.EnumProperty}&" +
                $"{nameof(expected.GuidProperty)}={expected.GuidProperty}&" +
                $"{nameof(expected.StringProperty)}={expected.StringProperty}");
        }

        [Test]
        public async Task When_TypeConverter_Exists_Then_Populated()
        {
            // Arrange
            var expected = new QueryTestOperation
            {
                Length = new Length
                {
                    Value = 154,
                    Unit = Unit.cm,
                },
            };

            // Act / Assert
            await AssertPopulatedFromQueryString(
                expected,
                $"?{nameof(expected.Length)}={expected.Length}");
        }

        [Test]
        public async Task When_TypeConverter_Exists_Then_Array_Populated()
        {
            // Arrange
            var lengths = new []
            {
                new Length
                {
                    Value = 14,
                    Unit = Unit.cm,
                },
                new Length
                {
                    Value = 89,
                    Unit = Unit.mm,
                },
            };

            var expected = new QueryTestOperation
            {
                Lengths = lengths,
            };

            // Act / Assert
            await AssertPopulatedFromQueryString(
                expected,
                $"?{nameof(expected.Lengths)}[]={lengths[0]}&{nameof(expected.Lengths)}[]={lengths[1]}");
        }

        [Test]
        public async Task When_Array_Like_As_Separate_Values_Then_Populates()
        {
            // Arrange
            var source = new List<string> { "arr1", "arr5" };

            string AsQuery(string key)
            {
                return $"{key}[]=arr1&{key}[]=arr5";
            }

            var expected = new QueryTestOperation
            {
                StringArray = source.ToArray(),
                StringEnumerable = source,
                StringList = source,
            };

            // Act / Assert
            await AssertPopulatedFromQueryString(
                expected,
                $"?{AsQuery(nameof(expected.StringArray))}&" +
                $"{AsQuery(nameof(expected.StringEnumerable))}&" +
                $"{AsQuery(nameof(expected.StringList))}");
        }

        [Test]
        public async Task When_Integer_Array_Like_As_Separate_Values_Then_Populates()
        {
            // Arrange
            var source = new List<int> { 1, 5, 199 };

            string AsQuery(string key)
            {
                return $"{key}[]=1&{key}[]=5&{key}[]=199";
            }

            var expected = new QueryMultiValueOperation<int>()
            {
                Array = source.ToArray(),
                Enumerable = source,
                List = source,
            };

            // Act / Assert
            await AssertPopulatedFromQueryString(
                expected,
                $"?{AsQuery(nameof(expected.Array))}&" +
                $"{AsQuery(nameof(expected.Enumerable))}&" +
                $"{AsQuery(nameof(expected.List))}");
        }

        private static async Task AssertPopulatedFromQueryString<TOperation>(TOperation expected, string queryString = null)
        {
            // Arrange
            var handler = new TestApiOperationHandler<TOperation>(null);
            var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler));

            var context = GetContext<TOperation>(executor, queryString);

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
        }

        private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, string queryString)
        {
            return executor.HttpContextFor<T>(ctx => ctx.Request.QueryString = new QueryString(queryString));
        }
    }
}
