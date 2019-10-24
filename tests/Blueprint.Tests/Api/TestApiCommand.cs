using Blueprint.Api;
using Blueprint.Core.Auditing;

namespace Blueprint.Tests.Api
{
    public class TestApiCommand : ICommand
    {
        public string AStringProperty { get; set; }

        [Sensitive]
        public string ASensitiveStringProperty { get; set; }
    }
}
