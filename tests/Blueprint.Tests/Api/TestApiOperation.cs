using System;
using System.Net.Http;
using Blueprint.Api;

namespace Blueprint.Tests.Api
{
    public class TestApiOperation : IApiOperation
    {
        public int Id { get; set; }

        public static ApiOperationContext NewOperationContext(IServiceProvider serviceProvider)
        {
            return ApiOperationContextSetup.CreateFromDescriptor(serviceProvider, NewDescriptor());
        }

        private static ApiOperationDescriptor NewDescriptor()
        {
            return new ApiOperationDescriptor(typeof(TestApiOperation), HttpMethod.Get);
        }
    }
}
