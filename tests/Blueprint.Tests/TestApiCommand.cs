using Blueprint.Auditing;

namespace Blueprint.Tests
{
    public class TestApiCommand : ICommand
    {
        public string AStringProperty { get; set; }

        [Sensitive]
        public string ASensitiveStringProperty { get; set; }

        [DoNotAudit]
        public string ADoNotAuditProperty { get; set; }

        public string ANakedPasswordProperty { get; set; }
    }
}
