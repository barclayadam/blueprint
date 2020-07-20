using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware
{
    public class Given_Cookie_Values
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        [RootLink("/cookie-values")]
        public class CookieTestOperation : IApiOperation
        {
            [FromCookie]
            public int IntegerProperty { get; set; }

            [FromCookie]
            public int? NullableIntegerProperty { get; set; }

            [FromCookie]
            public string StringProperty { get; set; }

            [FromCookie]
            public OperationEnum EnumProperty { get; set; }

            [FromCookie]
            public Guid GuidProperty { get; set; }

            [FromCookie]
            public IEnumerable<string> StringEnumerable { get; set; }

            [FromCookie]
            public string[] StringArray { get; set; }

            [FromCookie]
            public List<string> StringList { get; set; }
        }

        [Test]
        public async Task When_Simple_Properties_Then_Populated()
        {
            // Arrange
            var expected = new CookieTestOperation
            {
                IntegerProperty = 761,
                EnumProperty = OperationEnum.EnumOne,
                GuidProperty = Guid.NewGuid(),
                StringProperty = "string",
                NullableIntegerProperty = null
            };

            // Act / Assert
            await AssertCookies(
                expected,
                new Dictionary<string, string>
                {
                    [nameof(expected.IntegerProperty)] = expected.IntegerProperty.ToString(),
                    [nameof(expected.EnumProperty)] = expected.EnumProperty.ToString(),
                    [nameof(expected.GuidProperty)] = expected.GuidProperty.ToString(),
                    [nameof(expected.StringProperty)] = expected.StringProperty.ToString(),
                });
        }

        private static async Task AssertCookies<TOperation>(
            TOperation expected,
            Dictionary<string, string> headers = null) where TOperation : IApiOperation
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
            Dictionary<string, string> cookies) where T : IApiOperation
        {
            var context = executor.HttpContextFor<T>();
            var cookiesAsHeader = cookies.Select(c => $"{c.Key}={c.Value}").ToArray();

            context.GetHttpContext().Request.Headers["Cookie"] = cookiesAsHeader;

            return context;
        }
    }
}
