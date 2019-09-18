using System.Threading.Tasks;
using Blueprint.Api;

namespace Blueprint.Tests.Api.Validator_Middleware
{
    public class TestApiOperationHandler<T> : IApiOperationHandler<T> where T : IApiOperation
    {
        public TestApiOperationHandler(object toReturn)
        {
            ResultToReturn = toReturn;
        }

        public object ResultToReturn { get; }

        public bool WasCalled { get; private set; }

        public T OperationPassed { get; private set; }

        public ApiOperationContext ContextPassed { get; private set; }

        public Task<object> Invoke(T operation, ApiOperationContext apiOperationContext)
        {
            WasCalled = true;
            OperationPassed = operation;
            ContextPassed = apiOperationContext;

            return Task.FromResult(ResultToReturn);
        }
    }
}
