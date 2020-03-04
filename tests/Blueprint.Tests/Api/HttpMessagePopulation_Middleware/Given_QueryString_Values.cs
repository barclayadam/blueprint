﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Blueprint.Tests.Api.HttpMessagePopulation_Middleware
{
    public class Given_QueryString_Values
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [RootLink("/query-values")]
        public class QueryTestOperation : IApiOperation
        {
            public int IntegerProperty { get; set; }

            public int? NullableIntegerProperty { get; set; }

            public string StringProperty { get; set; }

            public OperationEnum EnumProperty { get; set; }

            public Guid GuidProperty { get; set; }

            public IEnumerable<string> StringEnumerable { get; set; }

            public string[] StringArray { get; set; }

            public List<string> StringList { get; set; }
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
            await AssertPopulatedFromRoute(
                expected,
                $"?{nameof(expected.IntegerProperty)}={expected.IntegerProperty}&" +
                $"{nameof(expected.EnumProperty)}={expected.EnumProperty}&" +
                $"{nameof(expected.GuidProperty)}={expected.GuidProperty}&" +
                $"{nameof(expected.StringProperty)}={expected.StringProperty}");
        }

        [Test]
        public async Task When_Array_Like_As_JSON_Arrays_Then_Populates()
        {
            // Arrange
            var source = new List<string> { "arr1", "arr5" };
            var asQuery = "[\"arr1\", \"arr5\"]";

            var expected = new QueryTestOperation
            {
                StringArray = source.ToArray(),
                StringEnumerable = source,
                StringList = source,
            };

            // Act / Assert
            await AssertPopulatedFromRoute(
                expected,
                $"?{nameof(expected.StringArray)}={asQuery}&" +
                $"{nameof(expected.StringEnumerable)}={asQuery}&" +
                $"{nameof(expected.StringList)}={asQuery}");
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
            await AssertPopulatedFromRoute(
                expected,
                $"?{AsQuery(nameof(expected.StringArray))}&" +
                $"{AsQuery(nameof(expected.StringEnumerable))}&" +
                $"{AsQuery(nameof(expected.StringList))}");
        }

        private static async Task AssertPopulatedFromRoute<TOperation>(TOperation expected, string queryString = null) where TOperation : IApiOperation
        {
            // Arrange
            var handler = new TestApiOperationHandler<TOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).Pipeline(p => p.AddHttp()));

            var context = GetContext<TOperation>(executor, queryString);

            // Act
            var result = await executor.ExecuteAsync(context);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
        }

        private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, string queryString) where T : IApiOperation
        {
            var context = executor.HttpContextFor<T>();
            context.Request.QueryString = new QueryString(queryString);

            return context;
        }
    }
}
