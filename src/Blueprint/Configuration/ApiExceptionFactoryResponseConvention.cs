using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blueprint.Configuration
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that uses XML documentation for the operation
    /// to add <see cref="ResponseDescriptor" />s, in particular handling the exception tag to
    /// add more details failure descriptions.
    /// </summary>
    public class ApiExceptionFactoryResponseConvention : IOperationScannerConvention
    {
        /// <inheritdoc />
        public void Apply(ApiOperationDescriptor descriptor)
        {
            var staticFields = descriptor.OperationType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var f in staticFields)
            {
                if (f.FieldType != typeof(ApiExceptionFactory))
                {
                    continue;
                }

                if (f.GetValue(null) is ApiExceptionFactory factory)
                {
                    descriptor.AddResponse(new ResponseDescriptor(
                        typeof(ApiException),
                        factory.HttpStatus,
                        factory.Title,
                        new Dictionary<string, string>
                        {
                            ["type"] = factory.Type,
                        }));
                }
            }
        }

        /// <inheritdoc />
        /// <returns><c>false</c>.</returns>
        public bool IsSupported(Type operationType)
        {
            return false;
        }
    }
}
