using System.Threading.Tasks;
using Blueprint.Api.Configuration;
using Blueprint.Api.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ApplicationInsights_Middleware
{
    public class Given_RegisteredMiddleware
    {
        [Test]
        public async Task When_ApplicationInsights_Added_Then_Can_Execute()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddApplicationInsights()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public void When_ApplicationInsights_Added_Then_Compiles()
        {
            // Arrange
            var handler = new TestApiOperationHandler<EmptyOperation>("87545");
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddApplicationInsights()));

            // Act
            executor.WhatCodeDidIGenerateFor<EmptyOperation>()
                .Should().Contain(".Features.Get<Microsoft.ApplicationInsights.DataContracts.RequestTelemetry>()");
        }
    }
}
