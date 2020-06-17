using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Apm.ElasticApm_Middleware
{
    public class Given_RegisteredMiddleware
    {
        [Test]
        public async Task When_ElasticApm_Added_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddElasticApm()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_ElasticApm_Added_With_Http_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddHttp().AddElasticApm()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_ElasticApm_Added_Then_Handler_Exception_Bubbled()
        {
            // Arrange
            var toThrow = new Exception("Oops");

            var handler = new TestApiOperationHandler<EmptyOperation>(toThrow);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddHttp().AddElasticApm()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            okResult.Exception.Should().Be(toThrow);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_ElasticApm_Added_Then_Sets_RequestTelemetry_Name()
        {
            // Arrange
            var transaction = Elastic.Apm.Agent.Tracer.StartTransaction("TestTransaction", "Test");

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddElasticApm()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
            transaction.Name.Should().Be(nameof(EmptyOperation));
        }

        [Test]
        public async Task When_ElasticApm_Added_Then_Sets_Success_False_On_Exception()
        {
            // Arrange
            var handler = new TestApiOperationHandler<EmptyOperation>(new Exception("Failure"));
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddElasticApm()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
        }

        [Test]
        public async Task When_ElasticApm_Added_With_User_Then_Sets_User_Context_Id_From_Sub_Claim()
        {
            // Arrange
            var transaction = Elastic.Apm.Agent.Tracer.StartTransaction("TestTransaction", "Test");

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddElasticApm())
                .Pipeline(p => p.AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            await executor.ExecuteWithAuth(
                new EmptyOperation(),
                new Claim("sub", "UserId12345"));

            // Assert
            transaction.Context.User.Id.Should().Be("UserId12345");
        }
    }
}
