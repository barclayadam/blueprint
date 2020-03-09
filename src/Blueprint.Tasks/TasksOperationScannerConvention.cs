using System;
using Blueprint.Api;
using Blueprint.Api.Configuration;

namespace Blueprint.Tasks
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that adds HTTP-related feature
    /// details to <see cref="ApiOperationDescriptor" />s and excludes any operations that
    /// have no <see cref="LinkAttribute" />.
    /// </summary>
    public class TasksOperationScannerConvention : IOperationScannerConvention
    {
        /// <inheritdoc />
        public void Apply(ApiOperationDescriptor descriptor)
        {
        }

        /// <inheritdoc />
        public bool ShouldInclude(Type operationType)
        {
            return typeof(IBackgroundTask).IsAssignableFrom(operationType);
        }
    }
}
