using System;
using System.Linq;
using System.Reflection;
using Blueprint.Configuration;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that adds HTTP-related feature
    /// details to <see cref="ApiOperationDescriptor" />s and excludes any operations that
    /// have no <see cref="LinkAttribute" />.
    /// </summary>
    public class HttpOperationScannerConvention : IOperationScannerConvention
    {
        /// <inheritdoc />
        public void Apply(ApiOperationDescriptor descriptor)
        {
            if (this.IsSupported(descriptor.OperationType))
            {
                string supportedMethod;

                var httpMethodAttribute = descriptor.OperationType.GetCustomAttribute<HttpMethodAttribute>(true);

                if (httpMethodAttribute != null)
                {
                    supportedMethod = httpMethodAttribute.HttpMethod;
                }
                else
                {
                    // By default, command are POST and everything else GET
                    supportedMethod = typeof(ICommand).IsAssignableFrom(descriptor.OperationType) ? "POST" : "GET";
                }

                descriptor.SetFeatureData(new HttpOperationFeatureData(supportedMethod));

                descriptor.AllowMultipleHandlers = false;
                descriptor.RequiresReturnValue = true;
            }
        }

        /// <inheritdoc />
        public bool IsSupported(Type operationType)
        {
            return operationType.GetCustomAttributes<LinkAttribute>().Any();
        }
    }
}
