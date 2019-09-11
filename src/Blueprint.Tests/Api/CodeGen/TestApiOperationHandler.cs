using System.Threading.Tasks;
using Blueprint.Api;

namespace Blueprint.Tests.Api.CodeGen
{
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
