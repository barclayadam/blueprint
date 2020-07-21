using System;
using System.Linq;

namespace Blueprint
{
    /// <summary>
    /// Exception thrown if operations have been registered that we could not find a
    /// handler for.
    /// </summary>
    public class MissingApiOperationHandlerException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MissingApiOperationHandlerException" /> class.
        /// </summary>
        /// <param name="missingApiOperationHandlers">All operations we could not find handlers for.</param>
        public MissingApiOperationHandlerException(ApiOperationDescriptor[] missingApiOperationHandlers)
            : base(BuildErrorMessage(missingApiOperationHandlers))
        {
            ApiOperationHandlers = missingApiOperationHandlers;
        }

        /// <summary>
        /// The operations that have no registered handlers.
        /// </summary>
        public ApiOperationDescriptor[] ApiOperationHandlers { get; }

        private static string BuildErrorMessage(ApiOperationDescriptor[] missingApiOperationHandlers)
        {
            return "Cannot find handlers for the following operations:\n" +
                   string.Join("\n", missingApiOperationHandlers.Select(o => o.OperationType.FullName));
        }
    }
}
