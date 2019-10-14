using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Testing;
using Blueprint.Tests.Api.Validator_Middleware;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Tests.Api.DependencyInjectionSource
{
    public class Given_DependencyInjection_Container
    {
        [Test]
        public async Task When_Empty_Operation_Then_Result_Executed()
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
                .WithMiddleware<MiddlewareWithDependencyInjectionVariable>());

            // Act
            await executor.ExecuteWithNewScopeAsync(new OperationWithInjectable());

            // Assert
            handler.OperationPassed.InjectableProperty.ShouldNotBeNull();
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
