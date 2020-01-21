using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Builder
{
    public class Given_DependencyInjection_Container
    {
        [Test]
        public async Task When_Middleware_Requests_Variable_Fulfilled_By_DI()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<OperationWithInjectable>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddSingleton(typeof(IInjectable), typeof(Injectable));
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable>(MiddlewareStage.Execution)));

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.Should().NotBeNull();
        }

        [Test]
        public void When_Singleton_Then_Injected_In_Constructor()
        {
            // Arrange
            var handler = new TestApiOperationHandler<OperationWithInjectable>(12345);

            // Act
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddSingleton(typeof(IInjectable), typeof(Injectable));
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable>(MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<OperationWithInjectable>();

            code.Should().NotContain("context.ServiceProvider.GetRequiredService<Blueprint.Tests.Api.Builder.Given_DependencyInjection_Container.IInjectable>();");
        }

        [Test]
        public void When_Transient_Then_GetRequiredService_At_Runtime()
        {
            // Arrange
            var handler = new TestApiOperationHandler<OperationWithInjectable>(12345);

            // Act
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddTransient(typeof(IInjectable), typeof(Injectable));
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable>(MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<OperationWithInjectable>();

            code.Should().Contain("context.ServiceProvider.GetRequiredService<Blueprint.Tests.Api.Builder.Given_DependencyInjection_Container.IInjectable>();");
        }

        [Test]
        public void When_Scoped_Then_GetRequiredService_At_Runtime()
        {
            // Arrange
            var handler = new TestApiOperationHandler<OperationWithInjectable>(12345);

            // Act
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddScoped(typeof(IInjectable), typeof(Injectable));
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable>(MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<OperationWithInjectable>();

            code.Should().Contain("context.ServiceProvider.GetRequiredService<Blueprint.Tests.Api.Builder.Given_DependencyInjection_Container.IInjectable>();");
        }

        [Test]
        public void When_Requesting_Singleton_Service_By_Interface_And_Concrete_Then_Chooses_Concrete_To_Inject()
        {
            // Arrange
            var handler = new TestApiOperationHandler<OperationWithInjectable>(12345);

            Action buildExecutor = () => TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddSingleton(typeof(IInjectable), typeof(Injectable));
                    s.AddSingleton(typeof(Injectable), typeof(Injectable));
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithMultipleDependencyInjectionVariable>(MiddlewareStage.Execution)));

            // Assert
            buildExecutor.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Contain("An attempt has been made to request a service form the DI container that will lead to a duplicate constructor argument.");
        }

        public class MiddlewareWithDependencyInjectionVariable : CustomFrameMiddlewareBuilder
        {
            public MiddlewareWithDependencyInjectionVariable() : base(false)
            {
            }

            public override bool Matches(ApiOperationDescriptor operation)
            {
                return operation.OperationType == typeof(OperationWithInjectable);
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationVariable = variables.FindVariable(typeof(OperationWithInjectable));
                var diVariable = variables.FindVariable(typeof(IInjectable));

                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diVariable};");
                next();
            }
        }

        public class MiddlewareWithMultipleDependencyInjectionVariable : CustomFrameMiddlewareBuilder
        {
            public MiddlewareWithMultipleDependencyInjectionVariable() : base(false)
            {
            }

            public override bool Matches(ApiOperationDescriptor operation)
            {
                return operation.OperationType == typeof(OperationWithInjectable);
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationVariable = variables.FindVariable(typeof(OperationWithInjectable));
                var diInterfaceVariable = variables.FindVariable(typeof(IInjectable));
                var diConcreteVariable = variables.FindVariable(typeof(Injectable));

                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diInterfaceVariable};");
                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diConcreteVariable};");

                next();
            }
        }

        public class OperationWithInjectable : ICommand
        {
            public IInjectable InjectableProperty { get; set; }
        }

        public interface IInjectable {}
        public class Injectable : IInjectable {}
    }
}
