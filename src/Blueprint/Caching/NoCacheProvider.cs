using System.Diagnostics.CodeAnalysis;

namespace Blueprint.Caching;

/// <summary>
/// Provides an implementation of <see cref="ICacheProvider"/> that is a no-op.
/// </summary>
public class NoCacheProvider : ICacheProvider
{
    /// <summary>
    /// Does nothing, the value will not be stored anywhere.
    /// </summary>
    /// <param name="key">
    /// The key used for identifying the value, not used.
    /// </param>
    /// <param name="value">
    /// The value to be stored, not used.
    /// </param>
    /// <param name="options">
    /// The options used to store the value, not used.
    /// </param>
    public void Add(string key, object value, CacheOptions options)
    {
    }

    /// <summary>
    /// Returns <c>false</c>, as no values are ever stored in the cache.
    /// </summary>
    /// <param name="key">
    /// The unique identifier of the item.
    /// </param>
    /// <returns>
    /// The value <c>false</c> on all occasions.
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type parameter used to determine unique key.")]
    public bool ContainsKey(string key)
    {
        return false;
    }

    /// <summary>
    /// Returns <c>null</c>, as nothing is ever stored.
    /// </summary>
    /// <param name="key">
    /// The key that was used to store a value.
    /// </param>
    /// <returns>
    /// <c>null</c>, as nothing ever stored.
    /// </returns>
    public object GetValue(string key)
    {
        return null;
    }

    /// <summary>
    /// Performs no action as no values are ever stored.
    /// </summary>
    /// <param name="key">
    /// The key to remove the value for.
    /// </param>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type parameter used to determine unique key.")]
    public void Remove(string key)
    {
    }
}