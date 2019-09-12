using System.Net.Http;
using Blueprint.Api;

namespace Blueprint.Tests.Api 
{
    public class TestApiCommand : ICommand
    {
        public static ApiOperationDescriptor NewDescriptor(string url = "/any")
        {
            return new ApiOperationDescriptor(typeof(TestApiCommand), HttpMethod.Post);
        }
    }
}
