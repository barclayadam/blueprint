﻿using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Middleware;
using Blueprint.Compiler;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.CodeGen
{
    public class ApiOperationExecutorTests
    {
        [Test]
        public async Task When_Handler_For_Operation_Exists_In_Container_Then_Call_Invoke_Method()
        {
            // Arrange
            var handler = new TestApiOperationHandler();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IApiOperationHandler<TestApiOperation>>(handler);

            serviceCollection.AddBlueprintApi((o) =>
            {
                o.Rules.CompileStrategy = new InMemoryOnlyCompileStrategy();

                o.WithApplicationName("Blueprint.Tests");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.AddOperation<TestApiOperation>();
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var apiOperationExecutor = serviceProvider.GetRequiredService<IApiOperationExecutor>();

            var ctx = TestApiOperation.NewOperationContext(serviceProvider);

            // Act
            await apiOperationExecutor.ExecuteAsync(ctx);

            // Assert
            handler.WasCalled.Should().BeTrue();
            handler.OperationPassed.Should().Be(ctx.Operation);

            // Simple check for a context that looks right, not extensive testing here!
            handler.ContextPassed.Should().NotBeNull();
            handler.ContextPassed.Should().Be(ctx);
        }
    }
}
