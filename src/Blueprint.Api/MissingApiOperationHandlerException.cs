using System;
using System.Linq;

namespace Blueprint.Api
{
    public class MissingApiOperationHandlerException : Exception
    {
        public MissingApiOperationHandlerException(ApiOperationDescriptor[] missingApiOperationHandlers)
            : base($"Cannot find handlers for the following operations: {string.Join(",", missingApiOperationHandlers.Select(o => o.OperationType.FullName))}")
        {
            ApiOperationHandlers = missingApiOperationHandlers;
        }

        public ApiOperationDescriptor[] ApiOperationHandlers { get; }
    }
}
