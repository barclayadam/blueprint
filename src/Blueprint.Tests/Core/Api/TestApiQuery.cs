using System.Net.Http;
using Blueprint.Api;
using StructureMap;

namespace Blueprint.Tests.Core.Api 
{
    public class TestApiQuery : IQuery
    {
        public static ApiOperationDescriptor NewDescriptor(string url = "/any")
        {
            return new ApiOperationDescriptor(typeof(TestApiQuery), HttpMethod.Get);
        }

        public static ApiOperationContext NewOperationContext(Container container)
        {
            return ApiOperationContextSetup.CreateFromDescriptor(container, NewDescriptor());
        }
    }
}
