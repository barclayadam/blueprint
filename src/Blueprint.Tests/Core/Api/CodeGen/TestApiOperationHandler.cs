using Blueprint.Api;

namespace Blueprint.Tests.Core.Api.CodeGen
{
    using System.Threading.Tasks;

    public class TestApiOperationHandler : IApiOperationHandler<TestApiOperation>
    {
        public bool WasCalled { get; private set; }
        public TestApiOperation OperationPassed { get; private set; }
        public ApiOperationContext ContextPassed { get; private set; }

        public Task<object> Invoke(TestApiOperation operation, ApiOperationContext apiOperationContext)
        {
            WasCalled = true;
            OperationPassed = operation;
            ContextPassed = apiOperationContext;

            return Task.FromResult(new object());
        }
    }
}
