using System.Reflection;
using Blueprint.Api.Configuration;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An <see cref="IOperationScannerFeatureContributor" /> that adds HTTP-related feature
    /// details to <see cref="ApiOperationDescriptor" />s.
    /// </summary>
    public class HttpOperationScannerFeatureContributor : IOperationScannerFeatureContributor
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
    }
}
