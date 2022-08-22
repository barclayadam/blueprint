﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Utilities;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware;

public class Given_Json_Body
{
    public enum OperationEnum
    {
        EnumOne,
        EnumTwo
    }

    [HttpPost]
    [RootLink("/a-static-route")]
    public class JsonOperation
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
    [RootLink("/a-static-route")]
    public class JsonOperationUsingFromBody
    {
        [FromBody]
        public JsonOperation JsonOperation { get; set; }
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
    public class MultipleRouteOperation
    {
        public string RouteProperty { get; set; }
    }

    [Test]
    [TestCaseSource(nameof(GetHttpConfiguration))]
    public async Task When_Json_Body_Then_Populates(Labelled<Action<BlueprintHttpBuilder>> configureHttp)
    {
        // Arrange
        var expected = new JsonOperation
        {
            IntegerProperty = 761,
            EnumProperty = OperationEnum.EnumOne,
            GuidProperty = Guid.NewGuid(),
            StringArray = new[] { "arr1", "arr2" },
            StringEnumerable = new[] { "arr3", "arr4" },
            StringList = new List<string> { "arr5", "arr6" },
            StringProperty = "a string",
            NullableIntegerProperty = null,
            StringIntegerDictionary = new Dictionary<string, int> { { "one", 1 }, { "twice", 12 } }
        };

        var handler = new TestApiOperationHandler<JsonOperation>(null);
        var executor = TestApiOperationExecutor.CreateHttp(
            o => o.WithHandler(handler),
            configureHttp: configureHttp);
        var context = GetContext(executor, expected);

        // Act
        await executor.ExecuteAsync(context);

        // Assert
        handler.OperationPassed.Should().BeEquivalentTo(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetHttpConfiguration))]
    public async Task When_Json_Body_And_FromBody_Attribute_Then_Populates(Labelled<Action<BlueprintHttpBuilder>> configureHttp)
    {
        // Arrange
        var expectedBody = new JsonOperation
        {
            IntegerProperty = 761,
            EnumProperty = OperationEnum.EnumOne,
            GuidProperty = Guid.NewGuid(),
            StringArray = new[] { "arr1", "arr2" },
            StringEnumerable = new[] { "arr3", "arr4" },
            StringList = new List<string> { "arr5", "arr6" },
            StringProperty = "a string",
            NullableIntegerProperty = null,
            StringIntegerDictionary = new Dictionary<string, int> { { "one", 1 }, { "twice", 12 } }
        };

        var handler = new TestApiOperationHandler<JsonOperationUsingFromBody>(null);
        var executor = TestApiOperationExecutor.CreateHttp(
            o => o.WithHandler(handler),
            configureHttp: configureHttp);

        var jsonBody = JsonConvert.SerializeObject(expectedBody);

        var context = executor.HttpContextFor<JsonOperationUsingFromBody>();
        context.GetHttpContext().Request.Body = jsonBody.AsUtf8Stream();
        context.GetHttpContext().Request.Headers["Content-Type"] = "application/json";

        // Act
        await executor.ExecuteAsync(context);

        // Assert
        handler.OperationPassed.Should().BeEquivalentTo(new JsonOperationUsingFromBody
        {
            JsonOperation = expectedBody,
        });
    }

    [Test]
    [TestCaseSource(nameof(GetHttpConfiguration))]
    public async Task When_Malformed_JSON_Then_Throws_ApiException(Labelled<Action<BlueprintHttpBuilder>> configureHttp)
    {
        // Arrange
        var handler = new TestApiOperationHandler<JsonOperation>(null);
        var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler), configureHttp: configureHttp);

        var context = executor.HttpContextFor<JsonOperation>();
        context.GetHttpContext().Request.Body = "{..}".AsUtf8Stream();
        context.GetHttpContext().Request.Headers["Content-Type"] = "application/json";

        // Act
        Func<Task> tryExecute = () => executor.ExecuteAsync(context);

        // Assert
        await tryExecute.Should()
            .ThrowApiExceptionAsync()
            .WithType("invalid_json")
            .WithTitle("The JSON payload is invalid");
    }

    [Test]
    [TestCaseSource(nameof(GetHttpConfiguration))]
    public async Task When_Route_Property_In_Json_Body_Then_Route_Value_Wins(Labelled<Action<BlueprintHttpBuilder>> configureHttp)
    {
        // Arrange
        var expected = new JsonWithRouteOperation
        {
            RouteProperty = "unexpectedRouteValue",
            IntegerProperty = 761,
            EnumProperty = OperationEnum.EnumOne,
            GuidProperty = Guid.NewGuid(),
            StringArray = new[] { "arr1", "arr2" },
            StringEnumerable = new[] { "arr3", "arr4" },
            StringList = new List<string> { "arr5", "arr6" },
            StringProperty = "a string",
            NullableIntegerProperty = null,
            StringIntegerDictionary = new Dictionary<string, int> { { "one", 1 }, { "twice", 12 } }
        };

        var expectedRouteValue = "theRouteValue";

        var handler = new TestApiOperationHandler<JsonWithRouteOperation>(null);
        var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler), configureHttp: configureHttp);
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
    [TestCaseSource(nameof(GetHttpConfiguration))]
    public async Task When_Route_Property_In_Json_Body_But_Not_RouteData_With_Multiple_Routes_Then_Sets(Labelled<Action<BlueprintHttpBuilder>> configureHttp)
    {
        // Arrange
        var expected = new MultipleRouteOperation { RouteProperty = "expectedValue" };

        var handler = new TestApiOperationHandler<MultipleRouteOperation>(null);
        var executor = TestApiOperationExecutor.CreateHttp(o => o.WithHandler(handler), configureHttp: configureHttp);
        var context = GetContext(executor, expected);

        // Act
        var source = executor.WhatCodeDidIGenerate();

        await executor.ExecuteAsync(context);

        // Assert
        handler.OperationPassed.Should().BeEquivalentTo(expected);
        handler.OperationPassed.RouteProperty.Should().Be(expected.RouteProperty);
    }

    private static IEnumerable<Labelled<Action<BlueprintHttpBuilder>>> GetHttpConfiguration()
    {
        yield return new ("System.Text",  _ => { });
        yield return new ("Newtonsoft",  o => { o.UseNewtonsoft(); });
    }

    private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, T body)
    {
        var jsonBody = JsonConvert.SerializeObject(body);

        return executor.HttpContextFor<T>(ctx =>
        {
            ctx.Request.Body = jsonBody.AsUtf8Stream();
            ctx.Request.Headers["Content-Type"] = "application/json";
        });
    }
}