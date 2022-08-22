using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware;

public class Given_Cookie_Values
{
    public enum OperationEnum
    {
        EnumOne,
        EnumTwo
    }

    [RootLink("/cookie-values")]
    public class CookieTestOperation
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
        public Length Length { get; set; }
    }

    [RootLink("/invalid-cookie-type")]
    public class InvalidArrayCookieOperation
    {
        [FromCookie]
        public Length[] Lengths { get; set; }
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

    [Test]
    public async Task When_TypeConverter_Exists_Then_Populated()
    {
        // Arrange
        var expected = new CookieTestOperation
        {
            Length = new Length
            {
                Value = 154,
                Unit = Unit.cm,
            },
        };

        // Act / Assert
        await AssertCookies(
            expected,
            new Dictionary<string, string>
            {
                [nameof(expected.Length)] = expected.Length.ToString(),
            });
    }

    [Test]
    public void When_Array_Like_Then_Exception()
    {
        // Arrange
        var handler = new TestApiOperationHandler<InvalidArrayCookieOperation>(null);

        // Act
        Action executor = () => TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler));

        // Assert
        executor.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Cannot create decoder for property InvalidArrayCookieOperation.Lengths as it is array-like and FromCookie does not support multiple values.");
    }

    private static async Task AssertCookies<TOperation>(
        TOperation expected,
        Dictionary<string, string> headers = null)
    {
        // Arrange
        var handler = new TestApiOperationHandler<TOperation>(null);
        var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler));

        var context = GetContext<TOperation>(executor, headers);

        // Act
        await executor.ExecuteAsync(context);

        // Assert
        handler.OperationPassed.Should().BeEquivalentTo(expected);
    }

    private static ApiOperationContext GetContext<T>(
        TestApiOperationExecutor executor,
        Dictionary<string, string> cookies)
    {
        return executor.HttpContextFor<T>(ctx =>
        {
            var cookiesAsHeader = cookies.Select(c => $"{c.Key}={c.Value}").ToArray();

            ctx.Request.Headers["Cookie"] = cookiesAsHeader;
        });
    }
}