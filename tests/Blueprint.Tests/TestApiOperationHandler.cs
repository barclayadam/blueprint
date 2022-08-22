using System;
using System.Threading.Tasks;

namespace Blueprint.Tests;

public class TestApiOperationHandler<T> : IApiOperationHandler<T>
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

    public ValueTask<object> Handle(T operation, ApiOperationContext apiOperationContext)
    {
        WasCalled = true;
        OperationPassed = operation;
        ContextPassed = apiOperationContext;

        if (ToThrow != null)
        {
            throw ToThrow;
        }

        return new ValueTask<object>(ResultToReturn);
    }
}