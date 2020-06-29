using System.Diagnostics.CodeAnalysis;

namespace Blueprint.Caching
{
    /// <summary>
    /// A caching strategy is used to determine how an object is stored in a cache, if
    /// at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When caching a value using the <see cref="ICache"/> service the option exists to
    /// specify the options by which a value should be cached. The alternative, and
    /// preferred approach, would be for the client to not worry about how a value
    /// may be cached. In this instance no options are passed and they are instead determined
    /// by an external strategy.
    /// </para>
    /// <para>
    /// It is the job of the strategy to determine how, and if, a value is to be stored
    /// in a cache. A default strategy should be expected to be injected in cases where
    /// no explicit strategy exists, with the default implementation to be not to cache
    /// any values.
    /// </para>
    /// </remarks>
    public interface ICachingStrategy
    {
        /// <summary>
        /// Gets the priority of this caching strategy, which is used when determining what
        /// strategy to use should more than one be able to handle a given value.
        /// The highest priority strategy will be picked, in the case of a tie the first registered strategy wins.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets a value which indicates whether or not this strategy could handle the
        /// specified key / value pair.
        /// </summary>
        /// <param name="category">
        /// A category that can be used to allow applying the same
        /// strategy to a group of related values (e.g. 'Content').
        /// </param>
        /// <param name="value">
        /// The value being inserted.
        /// </param>
        /// <returns>
        /// Whether or not this strategy will apply to the given pair.
        /// </returns>
        bool AppliesTo(string category, object value);

        /// <summary>
        /// Gets the options used to store the given key and value pair, returning
        /// CacheOptions.NotCached to indicate the value should not be cached.
        /// </summary>
        /// <returns>
        /// The options used to store the given key-value pair.
        /// </returns>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This is not normally a simple property but is usually a calculated value (e.g. based on current time)")]
        CacheOptions GetOptions();
    }
}
