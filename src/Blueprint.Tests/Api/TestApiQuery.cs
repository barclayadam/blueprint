using System.Net.Http;
using Blueprint.Api;

namespace Blueprint.Tests.Api
{
    public class TestApiQuery : IQuery
    {
        public static ApiOperationDescriptor NewDescriptor(string url = "/any")
        {
            return new ApiOperationDescriptor(typeof(TestApiQuery), HttpMethod.Get);
        }
    }
}
