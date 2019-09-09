namespace Blueprint.Core.Tracing
{
    public class NulloVersionInfoProvider : IVersionInfoProvider
    {
        public VersionInfo Value { get; } = new VersionInfo() { AppName = "Unknown" };
    }
}