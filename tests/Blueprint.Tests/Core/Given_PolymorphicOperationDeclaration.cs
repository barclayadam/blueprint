using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core
{
    public class Given_PolymorphicOperationDeclaration
    {
        [Test]
        public async Task When_Interface_Operation_Registered_Concrete_Operation_Can_Be_Executed()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateStandalone(o => o
                    .WithHandler(new OperationImplHandler())
                    .WithOperation<IOperationInterface>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new OperationImpl());

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        public interface IOperationInterface {}

        public class OperationImpl : IOperationInterface
        {
        }

        public class OperationImplHandler : IApiOperationHandler<IOperationInterface>
        {
            public ValueTask<object> Handle(IOperationInterface iOperation, ApiOperationContext apiOperationContext)
            {
                return default;
            }
        }
    }
}
