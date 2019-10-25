using System.Collections.Generic;
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

        public class MiddlewareWithDependencyInjectionVariable : CustomFrameMiddlewareBuilder
        {
            private Variable diVariable;
            private Variable operationVariable;

            public MiddlewareWithDependencyInjectionVariable() : base(false)
            {
            }

            public override bool Matches(ApiOperationDescriptor operation)
            {
                return operation.OperationType == typeof(OperationWithInjectable);
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diVariable};");
                Next?.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return operationVariable = chain.FindVariable(typeof(OperationWithInjectable));
                yield return diVariable = chain.FindVariable(typeof(IInjectable));
            }
        }

        public class OperationWithInjectable : ICommand
        {
            public IInjectable InjectableProperty { get; set; }
        }

        public class IInjectable {}
        public class Injectable : IInjectable {}
    }
}
