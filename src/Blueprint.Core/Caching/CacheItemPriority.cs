namespace Blueprint.Core.Caching
{
    /// <summary>
    /// Specifies the relative priority of items stored in an <see cref="ICache"/>.
    /// </summary>
    public enum CacheItemPriority
    {
        /// <summary>
        /// Cache items with this priority level are the most likely to be deleted from the cache as the server frees system memory.
        /// </summary>
        Low,

        /// <summary>
        /// Cache items with this priority level are likely to be deleted from the cache as the server frees system memory only
        /// after those items with <see cref="Low"/> priority. This is the default.
        /// </summary>
        Medium,

        /// <summary>
        /// Cache items with this priority level are the least likely to be deleted from the cache as the server frees system memory.
        /// </summary>
        High,
    }
}
