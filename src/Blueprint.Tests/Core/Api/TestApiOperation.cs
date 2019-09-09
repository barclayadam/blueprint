using System.Net.Http;
using Blueprint.Api;
using StructureMap;

namespace Blueprint.Tests.Core.Api
{
    public class TestApiOperation : IApiOperation
    {
        public int Id { get; set; }

        public static ApiOperationDescriptor NewDescriptor()
        {
            return new ApiOperationDescriptor(typeof(TestApiOperation), HttpMethod.Get);
        }

        public static ApiOperationContext NewOperationContext(IContainer container)
        {
            return ApiOperationContextSetup.CreateFromDescriptor(container, NewDescriptor());
        }
    }
}
