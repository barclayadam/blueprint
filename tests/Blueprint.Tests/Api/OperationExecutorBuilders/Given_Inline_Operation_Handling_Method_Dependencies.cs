using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.OperationExecutorBuilders
{
    public class Given_Inline_Operation_Handling_Method_Dependencies
    {
        [Test]
        public async Task When_Dependency_From_Operation_Context_And_IoC_Then_Injects()
        {
            // Arrange
            var operation = new InlineHandle();
            var executor = TestApiOperationExecutor.CreateStandalone(
                o => o.WithOperation<InlineHandle>(),
                s => s.AddTransient<IDependency, Dependency>());

            // Act
            await executor.ExecuteWithNewScopeAsync(operation);

            // Assert
            operation.Context.Should().NotBeNull();
            operation.Context.Operation.Should().Be(operation);

            operation.Dependency.Should().NotBeNull();
        }

        public interface IDependency {}

        public class Dependency : IDependency {}

        public class InlineHandle
        {
            public ApiOperationContext Context { get; set; }

            public IDependency Dependency { get; set; }

            public OkResult Handle(ApiOperationContext context, IDependency dependency)
            {
                Context = context;
                Dependency = dependency;

                return new OkResult(nameof(InlineHandle));
            }
        }
    }
}
