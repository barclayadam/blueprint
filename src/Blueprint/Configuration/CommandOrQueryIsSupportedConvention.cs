using System;

namespace Blueprint.Configuration
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that marks any class that implements
    /// <see cref="IQuery" /> or <see cref="ICommand" /> as a supported operation.
    /// </summary>
    public class CommandOrQueryIsSupportedConvention : IOperationScannerConvention
    {
        /// <inheritdoc />
        public void Apply(ApiOperationDescriptor descriptor)
        {
            // Nothing to do
        }

        /// <inheritdoc />
        /// <returns><c>false</c>.</returns>
        public bool IsSupported(Type operationType)
        {
            return typeof(ICommand).IsAssignableFrom(operationType) || typeof(IQuery).IsAssignableFrom(operationType);
        }
    }
}
