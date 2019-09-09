namespace Blueprint.Core.Tracing
{
    /// <summary>
    /// Apps are expected to implement <see cref="IVersionInfoProvider" /> to provide a <see cref="VersionInfo" />
    /// object that describes the running application which is then used for tracing, for example in
    /// background tasks metadata.
    /// </summary>
    public interface IVersionInfoProvider
    {
        /// <summary>
        /// Gets the version information for the currently running application.
        /// </summary>
        VersionInfo Value { get; }
    }
}