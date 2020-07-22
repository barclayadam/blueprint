using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blueprint.Configuration
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that looks for public static instances of
    /// <see cref="ApiExceptionFactory" /> on the operation type to add equivalent
    /// <see cref="ResponseDescriptor" /> instances to the <see cref="ApiOperationDescriptor" />.
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
