namespace Blueprint.Api
{
    /// <summary>
    /// Determiens what should happen when a URL is hit within the 'subsection' defined for the
    /// API.
    /// </summary>
    public enum NotFoundMode
    {
        /// <summary>
        /// Handle all URLs prefixed with the specified path segment.
        /// </summary>
        Handle,

        /// <summary>
        /// <b>Does not</b> handle not-found URLs, allowing them to fall through the middleware and
        /// routing components to potentially be handled elsewhere, useful for integrating with
        /// existing applications that already have endpoints under the same path segment.
        /// </summary>
        Fallthrough
    }
}