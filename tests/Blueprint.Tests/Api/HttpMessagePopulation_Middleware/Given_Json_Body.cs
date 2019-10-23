using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Middleware;
using Blueprint.Core.Utilities;
using Blueprint.Testing;
using Blueprint.Tests.Api.Validator_Middleware;
using DeepEqual.Syntax;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Tests.Api.HttpMessagePopulation_Middleware
{
    public class Given_Json_Body
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [HttpPost]
        [RootLink("/route/{RouteProperty}")]
        public class JsonOperation : IApiOperation
        {
            public string RouteProperty { get; set; }

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
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).WithMiddleware<HttpMessagePopulationMiddlewareBuilder>());
            var context = GetContext(executor, expected);

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.ShouldDeepEqual(expected);
        }

        [Test]
        public async Task When_Route_Property_In_Json_Body_Then_Route_Value_Wins()
        {
            // Arrange
            var expected = new JsonOperation
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

            var handler = new TestApiOperationHandler<JsonOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).WithMiddleware<HttpMessagePopulationMiddlewareBuilder>());
            var context = GetContext(executor, expected);

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.WithDeepEqual(expected)
                .IgnoreSourceProperty(p => p.RouteProperty)
                .Assert();

            handler.OperationPassed.RouteProperty.ShouldBe(expectedRouteValue);
        }

        [Test]
        public async Task When_Route_Property_In_Json_Body_But_Not_RouteData_Then_Does_Not_Set()
        {
            // Arrange
            var expected = new JsonOperation
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

            var handler = new TestApiOperationHandler<JsonOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).WithMiddleware<HttpMessagePopulationMiddlewareBuilder>());
            var context = GetContext(executor, expected);

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.WithDeepEqual(expected)
                .IgnoreSourceProperty(p => p.RouteProperty)
                .Assert();

            handler.OperationPassed.RouteProperty.ShouldBeNull();
        }

        private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, T body) where T : IApiOperation
        {
            var jsonBody = JsonConvert.SerializeObject(body);

            var context = executor.HttpContextFor<T>();
            context.Request.Body = jsonBody.AsUtf8Stream();
            context.Request.Headers["Content-Type"] = "application/json";

            return context;
        }
    }
}
