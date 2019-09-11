using System.Net.Http;
using Blueprint.Api;
using StructureMap;

namespace Blueprint.Tests.Api 
{
    public class TestApiCommand : ICommand
    {
        public static ApiOperationDescriptor NewDescriptor(string url = "/any")
        {
            return new ApiOperationDescriptor(typeof(TestApiCommand), HttpMethod.Post);
        }

        public static ApiOperationContext NewOperationContext(IContainer container)
        {
            return ApiOperationContextSetup.CreateFromDescriptor(container, NewDescriptor());
        }
    }
}
