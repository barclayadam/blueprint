using System.Net.Http;
using Blueprint.Api;

namespace Blueprint.Tests.Core.Api 
{
    using StructureMap;

    public class TestApiCommand : ICommand
    {
        public static ApiOperationDescriptor NewDescriptor(string url = "/any")
        {
            return new ApiOperationDescriptor(typeof(TestApiCommand), HttpMethod.Post);
        }

        public static ApiOperationContext NewOperationContext(Container container)
        {
            return ApiOperationContextSetup.CreateFromDescriptor(container, NewDescriptor());
        }
    }
}
