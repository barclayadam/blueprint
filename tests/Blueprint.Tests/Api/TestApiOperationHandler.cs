using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Http;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Tests.Api
{
    public class TestApiOperationHandler<T> : IApiOperationHandler<T> where T : IApiOperation
    {
        public TestApiOperationHandler(object toReturn)
        {
            ResultToReturn = toReturn;
        }

        public TestApiOperationHandler(Exception toThrow)
        {
            ToThrow = toThrow;
        }

        public Exception ToThrow { get; }

        public object ResultToReturn { get; }

        public bool WasCalled { get; private set; }

        public T OperationPassed { get; private set; }

        public ApiOperationContext ContextPassed { get; private set; }

        public Task<object> Invoke(T operation, ApiOperationContext apiOperationContext)
        {
            var httpRequest = apiOperationContext.GetHttpContext().Request;

            WasCalled = true;
            OperationPassed = operation;
            ContextPassed = apiOperationContext;

            if (ToThrow != null)
            {
                throw ToThrow;
            }

            return Task.FromResult(ResultToReturn);
        }
    }
}
