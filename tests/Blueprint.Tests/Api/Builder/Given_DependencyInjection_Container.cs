using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IInjectable>>(MiddlewareStage.Execution)));

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.Should().NotBeNull();
        }

        [Test]
        public async Task When_Middleware_Requests_Variable_Fulfilled_By_Open_Generic_DI_Registration()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<OperationWithInjectable>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddOptions<MyOptions>();
                })
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IOptions<MyOptions>>>(MiddlewareStage.Execution)));

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.Should().NotBeNull();
        }

        [Test]
        public async Task When_Middleware_Requests_Multiple_Generic_Classes_With_Different_Type_Parameters_Then_Success()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<OperationWithInjectable>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithServices(s =>
                {
                    s.AddOptions<MyOptions>();
                    s.AddOptions<MyOtherOptions>();
                })
                .WithHandler(handler)
                .Pipeline(p => p
                    .AddMiddlewareBefore<MiddlewareWithMultipleDependencyInjectionVariable<IOptions<MyOptions>, IOptions<MyOtherOptions>>>(MiddlewareStage.Execution)));

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.Should().NotBeNull();
        }

        [Test]
        public async Task When_Middleware_Requests_Enumerable_Variable_With_Single_Registered_Service_Fulfilled_By_DI()
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
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IEnumerable<IInjectable>>>(MiddlewareStage.Execution)));

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.Should().NotBeNull();

            var propertyArray = (IEnumerable<IInjectable>)handler.OperationPassed.InjectableProperty;
            propertyArray.Should().HaveCount(1);
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
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IInjectable>>(MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<OperationWithInjectable>();

            code.Should().NotContain(
                "context.ServiceProvider.GetRequiredService<Blueprint.Tests.Api.Builder.Given_DependencyInjection_Container.IInjectable>();");
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
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IInjectable>>(MiddlewareStage.Execution)));

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
                .Pipeline(p => p.AddMiddlewareBefore<MiddlewareWithDependencyInjectionVariable<IInjectable>>(MiddlewareStage.Execution)));

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
                .Pipeline(p => p
                    .AddMiddlewareBefore<MiddlewareWithMultipleDependencyInjectionVariable<IInjectable, Injectable>>(MiddlewareStage.Execution)));

            // Assert
            buildExecutor.Should().Throw<InvalidOperationException>()
                .And.Message.Should()
                .Contain("An attempt has been made to request a service (Blueprint.Tests.Api.Builder.Given_DependencyInjection_Container+Injectable) " +
                         "from the DI container that will lead to a duplicate constructor argument");
        }

        private class MiddlewareWithDependencyInjectionVariable<T> : CustomFrameMiddlewareBuilder
        {
            public MiddlewareWithDependencyInjectionVariable() : base(false)
            {
            }

            public override bool SupportsNestedExecution => true;

            public override bool Matches(ApiOperationDescriptor operation)
            {
                return operation.OperationType == typeof(OperationWithInjectable);
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationVariable = variables.FindVariable(typeof(OperationWithInjectable));
                var diVariable = variables.FindVariable(typeof(T));

                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diVariable};");
                next();
            }
        }

        private class MiddlewareWithMultipleDependencyInjectionVariable<T1, T2> : CustomFrameMiddlewareBuilder
        {
            public MiddlewareWithMultipleDependencyInjectionVariable() : base(false)
            {
            }

            public override bool SupportsNestedExecution => true;

            public override bool Matches(ApiOperationDescriptor operation)
            {
                return operation.OperationType == typeof(OperationWithInjectable);
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationVariable = variables.FindVariable(typeof(OperationWithInjectable));
                var diInterfaceVariable = variables.FindVariable(typeof(T1));
                var diConcreteVariable = variables.FindVariable(typeof(T2));

                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diInterfaceVariable};");
                writer.Write($"{operationVariable}.{nameof(OperationWithInjectable.InjectableProperty)} = {diConcreteVariable};");

                next();
            }
        }

        [PublicAPI]
        public class OperationWithInjectable : ICommand
        {
            public object InjectableProperty { get; set; }
        }

        [PublicAPI]
        public interface IInjectable
        {
        }

        [PublicAPI]
        public class Injectable : IInjectable
        {
        }

        [PublicAPI]
        public class MyOptions
        {
        }

        [PublicAPI]
        public class MyOtherOptions
        {
        }
    }
}
