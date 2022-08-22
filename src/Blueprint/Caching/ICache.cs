using System;
using System.Diagnostics.CodeAnalysis;

namespace Blueprint.Caching;

/// <summary>
/// A cache, a data store which is designed to store expensive objects once they have been created,
/// for example from a database or web service call.
/// </summary>
/// <remarks>
/// The cache is not a simple proxy over an existing caching implementation (e.g. the HttpRuntime cache
/// or the Enterprise Library). Ideally clients would not need any knowledge of how values are stored,
/// or the options which determine their behaviour. The configuration details and strategies used to
/// determine whether and under what options an object is cached is outside the control of the client
/// using the cache.
/// </remarks>
/// <seealso cref="ICachingStrategy"/>
public interface ICache
{
    /// <summary>
    /// Adds an item to the cache.
    /// </summary>
    /// <remarks>
    /// If the value supplies is <c>null</c> this method becomes a no-op.
    /// </remarks>
    /// <param name="category">A category the value belongs to, used to specify 'profiles' of caching via.
    /// configuration such as 'Content', or 'Reports'.</param>
    /// <param name="key">The key used for identifying the value.</param>
    /// <param name="value">The value to be stored.</param>
    /// <typeparam name="T">The type of the value being inserted / retrieved, inferred by the return type
    /// of the constructor.</typeparam>
    void Add<T>(string category, object key, T value);

    /// <summary>
    /// Returns a value indicating whether or not the cache contains a value against
    /// the given key.
    /// </summary>
    /// <param name="key">The unique identifier of the item.</param>
    /// <typeparam name="T">The type of the value being checked for existence.</typeparam>
    /// <returns>Whether a value for the key exists within the cache.</returns>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to avoid key naming conflicts")]
    bool ContainsKey<T>(object key);

    /// <summary>
    /// Gets a value from this cache that has been previously stored using
    /// the specified key.
    /// </summary>
    /// <param name="key">
    /// The key that was used to store a value.
    /// </param>
    /// <typeparam name="T">
    /// The type of the object that has been stored.
    /// </typeparam>
    /// <returns>
    /// The value that had been previously stored for the key.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a value has been stored for
    /// this key that is not of type <typeparamref name="T"/>.
    /// </exception>
    T GetValue<T>(object key);

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">
    /// The key to remove the value for.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted / retrieved, inferred by the return type
    /// of the constructor.
    /// </typeparam>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to avoid key naming conflicts")]
    void Remove<T>(object key);
}