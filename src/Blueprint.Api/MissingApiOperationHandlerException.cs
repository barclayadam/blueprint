using System;

namespace Blueprint.Api
{
    public class MissingApiOperationHandlerException : Exception
    {
        public MissingApiOperationHandlerException(ApiOperationDescriptor[] missingApiOperationHandlers)
            : base($"There are missing ApiOperationHandlers. count={missingApiOperationHandlers.Length}")
        {
            ApiOperationHandlers = missingApiOperationHandlers;
        }

        public ApiOperationDescriptor[] ApiOperationHandlers { get; }
    }
}
