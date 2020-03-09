using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Blueprint.Api.Configuration;

namespace Blueprint.Api.Http
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

            descriptor.Name = supportedMethod + " " + descriptor.Name;
            descriptor.SetFeatureData(new HttpOperationFeatureData(supportedMethod));
        }

        /// <inheritdoc />
        public bool ShouldInclude(Type operationType)
        {
            return operationType.GetCustomAttributes<LinkAttribute>().Any();
        }
    }
}
