using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Utilities;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware
{
    public class Given_Json_Body
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [HttpPost]
        public class JsonOperation : IApiOperation
        {
            public int IntegerProperty { get; set; }

            public int? NullableIntegerProperty { get; set; }

            public string StringProperty { get; set; }

            public OperationEnum EnumProperty { get; set; }

            public Guid GuidProperty { get; set; }

            public IEnumerable<string> StringEnumerable { get; set; }

            public string[] StringArray { get; set; }

            public List<string> StringList { get; set; }

            public Dictionary<string, int> StringIntegerDictionary { get; set; }
        }

        [HttpPost]
        [RootLink("/route/{RouteProperty}")]
        public class JsonWithRouteOperation : JsonOperation
        {
            public string RouteProperty { get; set; }
        }

        [HttpPost]
        [RootLink("/route/{RouteProperty}")]
        [RootLink("/route/without-property")]
        public class JsonWithOptionalRouteOperation : JsonOperation
        {
            public string RouteProperty { get; set; }
        }

        [HttpPost]
        [RootLink("/route/{RouteProperty}")]
        [RootLink("/route-without-property")]
        public class MultipleRouteOperation : IApiOperation
        {
            public string RouteProperty { get; set; }
        }

        [Test]
        public async Task When_Json_Body_Then_Populates()
        {
            // Arrange
            var expected = new JsonOperation
            {
                IntegerProperty = 761,
                EnumProperty = OperationEnum.EnumOne,
                GuidProperty = Guid.NewGuid(),
                StringArray = new [] { "arr1", "arr2" },
                StringEnumerable = new [] {"arr3", "arr4" },
                StringList = new List<string> { "arr5", "arr6" },
                StringProperty = "a string",
                NullableIntegerProperty = null,
                StringIntegerDictionary = new Dictionary<string, int>
                {
                    { "one", 1 },
                    { "twice", 12 }
                }
            };

            var handler = new TestApiOperationHandler<JsonOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).Configure(p => p.AddHttp()));
            var context = GetContext(executor, expected);

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task When_Route_Property_In_Json_Body_Then_Route_Value_Wins()
        {
            // Arrange
            var expected = new JsonWithRouteOperation
            {
                RouteProperty = "unexpectedRouteValue",
                IntegerProperty = 761,
                EnumProperty = OperationEnum.EnumOne,
                GuidProperty = Guid.NewGuid(),
                StringArray = new [] { "arr1", "arr2" },
                StringEnumerable = new [] {"arr3", "arr4" },
                StringList = new List<string> { "arr5", "arr6" },
                StringProperty = "a string",
                NullableIntegerProperty = null,
                StringIntegerDictionary = new Dictionary<string, int>
                {
                    { "one", 1 },
                    { "twice", 12 }
                }
            };

            var expectedRouteValue = "theRouteValue";

            var handler = new TestApiOperationHandler<JsonWithRouteOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).Configure(p => p.AddHttp()));
            var context = GetContext(executor, expected);
            context.GetRouteData().Values[nameof(JsonWithRouteOperation.RouteProperty)] = expectedRouteValue;

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(
                expected,
                o => o.Excluding(x => x.RouteProperty));

            handler.OperationPassed.RouteProperty.Should().Be(expectedRouteValue);
        }

        [Test]
        public async Task When_Route_Property_In_Json_Body_But_Not_RouteData_With_Multiple_Routes_Then_Sets()
        {
            // Arrange
            var expected = new MultipleRouteOperation
            {
                RouteProperty = "expectedValue"
            };

            var handler = new TestApiOperationHandler<MultipleRouteOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).Configure(p => p.AddHttp()));
            var context = GetContext(executor, expected);

            // Act
            var source = executor.WhatCodeDidIGenerate();

            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
            handler.OperationPassed.RouteProperty.Should().Be(expected.RouteProperty);
        }

        private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, T body) where T : IApiOperation
        {
            var jsonBody = JsonConvert.SerializeObject(body);

            var context = executor.HttpContextFor<T>();
            context.GetHttpContext().Request.Body = jsonBody.AsUtf8Stream();
            context.GetHttpContext().Request.Headers["Content-Type"] = "application/json";

            return context;
        }
    }
}
