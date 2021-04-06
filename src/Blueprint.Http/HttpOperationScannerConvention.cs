using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Blueprint.Configuration;
using Blueprint.Middleware;

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

                RegisterResponses(descriptor);
            }
        }

        /// <inheritdoc />
        public bool IsSupported(Type operationType)
        {
            return operationType.GetCustomAttributes<LinkAttribute>().Any();
        }

        private static void RegisterResponses(ApiOperationDescriptor descriptor)
        {
            var typedOperation = descriptor.OperationType
                .GetInterfaces()
                .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReturn<>));

            if (typedOperation != null)
            {
                var returnType = typedOperation.GetGenericArguments()[0];

                // If we have a StatusCodeResult then we either
                // 1. Have a specific subclass and therefore know the expected response code and can therefore add a response
                // 2. Have the base class and therefore can not determine the actual expected response code so leave it open and do not add anything specific
                if (typeof(StatusCodeResult).IsAssignableFrom(returnType))
                {
                    var instanceProperty = returnType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

                    if (instanceProperty != null)
                    {
                        // This is option 1, we have a specific subclass (see the .tt file that generates these, i.e. StatusCodeResult.Created)
                        var statusCode = ((StatusCodeResult)instanceProperty.GetValue(null)).StatusCode;

                        descriptor.AddResponse(
                            new ResponseDescriptor((int)statusCode, statusCode.ToString()));
                    }
                }
                else
                {
                    descriptor.AddResponse(
                        new ResponseDescriptor(returnType, (int)HttpStatusCode.OK, HttpStatusCode.OK.ToString()));
                }
            }

            descriptor.AddResponse(
                new ResponseDescriptor(typeof(UnhandledExceptionOperationResult), 500, "Unexpected error"));

            descriptor.AddResponse(
                new ResponseDescriptor(typeof(ValidationFailedOperationResult), 422, "Validation failure"));
        }
    }
}
