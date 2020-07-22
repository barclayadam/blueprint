using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware
{
    public class Given_Header_Values
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [RootLink("/header-values")]
        public class HeaderTestOperation
        {
            [FromHeader]
            public int IntegerProperty { get; set; }

            [FromHeader]
            public int? NullableIntegerProperty { get; set; }

            [FromHeader]
            public string StringProperty { get; set; }

            [FromHeader]
            public OperationEnum EnumProperty { get; set; }

            [FromHeader]
            public Guid GuidProperty { get; set; }

            [FromHeader]
            public IEnumerable<string> StringEnumerable { get; set; }

            [FromHeader]
            public string[] StringArray { get; set; }

            [FromHeader]
            public List<string> StringList { get; set; }
        }

        [Test]
        public async Task When_Simple_Properties_Then_Populated()
        {
            // Arrange
            var expected = new HeaderTestOperation
            {
                IntegerProperty = 761,
                EnumProperty = OperationEnum.EnumOne,
                GuidProperty = Guid.NewGuid(),
                StringProperty = "a string",
                NullableIntegerProperty = null
            };

            // Act / Assert
            await AssertHeaders(
                expected,
                new Dictionary<string, string>
                {
                    [nameof(expected.IntegerProperty)] = expected.IntegerProperty.ToString(),
                    [nameof(expected.EnumProperty)] = expected.EnumProperty.ToString(),
                    [nameof(expected.GuidProperty)] = expected.GuidProperty.ToString(),
                    [nameof(expected.StringProperty)] = expected.StringProperty.ToString(),
                });
        }

        [Test]
        public async Task When_Array_Like_As_JSON_Arrays_Then_Populates()
        {
            // Arrange
            var source = new List<string> { "arr1", "arr5" };
            var asHeader = "[\"arr1\", \"arr5\"]";

            var expected = new HeaderTestOperation
            {
                StringArray = source.ToArray(),
                StringEnumerable = source,
                StringList = source,
            };

            // Act / Assert
            await AssertHeaders(
                expected,
                new Dictionary<string, string>
                    {
                        [nameof(expected.StringArray)] = asHeader,
                        [nameof(expected.StringEnumerable)] = asHeader,
                        [nameof(expected.StringList)] = asHeader,
                    });
        }

        private static async Task AssertHeaders<TOperation>(
            TOperation expected,
            Dictionary<string, string> headers = null)
        {
            // Arrange
            var handler = new TestApiOperationHandler<TOperation>(null);
            var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler));

            var context = GetContext<TOperation>(executor, headers);

            // Act
            var result = await executor.ExecuteAsync(context);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
        }

        private static ApiOperationContext GetContext<T>(
            TestApiOperationExecutor executor,
            Dictionary<string, string> headers)
        {
            var context = executor.HttpContextFor<T>();

            foreach (var h in headers)
            {
                context.GetHttpContext().Request.Headers[h.Key] = h.Value;
            }

            return context;
        }
    }
}
