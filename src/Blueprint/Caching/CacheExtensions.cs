using System;
using System.Threading.Tasks;

namespace Blueprint.Caching;

/// <summary>
/// Extensions class that augments the base <see cref="ICache"/> interface with the <see cref="GetOrCreate{T}(ICache,string,object,System.Func{T})"/>
/// method that can handle the boilerplate associated with cache access that involves retrieving a value and
/// storing if not available.
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// Gets a value from this cache, constructing and storing a value should the cache
    /// not contain an item with the specified key.
    /// </summary>
    /// <param name="cache">
    /// The cache to work against.
    /// </param>
    /// <param name="key">
    /// The key of the value being retrieved.
    /// </param>
    /// <param name="constructor">
    /// A function that will construct the value to store in the cache if
    /// a value cannot be found.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted / retrieved, inferred by the return type
    /// of the constructor.
    /// </typeparam>
    /// <returns>
    /// The cached value stored for the given key, or the result of executing the constructor
    /// function.
    /// </returns>
    public static T GetOrCreate<T>(this ICache cache, object key, Func<T> constructor)
    {
        Guard.NotNull(nameof(cache), cache);
        Guard.NotNull(nameof(key), key);
        Guard.NotNull(nameof(constructor), constructor);

        return cache.GetOrCreate(null, key, constructor);
    }

    /// <summary>
    /// Gets a value from this cache, constructing and storing a value should the cache
    /// not contain an item with the specified key.
    /// </summary>
    /// <param name="cache">
    /// The cache to work against.
    /// </param>
    /// <param name="category">
    /// A category the value belongs to, used to specify 'profiles' of caching via.
    /// configuration such as 'Content', or 'Reports'.
    /// </param>
    /// <param name="key">
    /// The key of the value being retrieved.
    /// </param>
    /// <param name="constructor">
    /// A function that will construct the value to store in the cache if
    /// a value cannot be found.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted / retrieved, inferred by the return type
    /// of the constructor.
    /// </typeparam>
    /// <returns>
    /// The cached value stored for the given key, or the result of executing the constructor
    /// function.
    /// </returns>
    public static T GetOrCreate<T>(this ICache cache, string category, object key, Func<T> constructor)
    {
        Guard.NotNull(nameof(cache), cache);
        Guard.NotNull(nameof(key), key);
        Guard.NotNull(nameof(constructor), constructor);

        if (!cache.ContainsKey<T>(key))
        {
            var value = constructor();
            cache.Add(category, key, value);

            return value;
        }

        return cache.GetValue<T>(key);
    }

    /// <summary>
    /// Gets a value from this cache, constructing and storing a value should the cache
    /// not contain an item with the specified key.
    /// </summary>
    /// <param name="cache">
    /// The cache to work against.
    /// </param>
    /// <param name="category">
    /// A category the value belongs to, used to specify 'profiles' of caching via.
    /// configuration such as 'Content', or 'Reports'.
    /// </param>
    /// <param name="key">
    /// The key of the value being retrieved.
    /// </param>
    /// <param name="constructor">
    /// A function that will construct the value to store in the cache if
    /// a value cannot be found.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted / retrieved, inferred by the return type
    /// of the constructor.
    /// </typeparam>
    /// <returns>
    /// The cached value stored for the given key, or the result of executing the constructor
    /// function.
    /// </returns>
    public static async Task<T> GetOrCreateAsync<T>(this ICache cache, string category, object key, Func<Task<T>> constructor)
    {
        Guard.NotNull(nameof(cache), cache);
        Guard.NotNull(nameof(key), key);
        Guard.NotNull(nameof(constructor), constructor);

        if (!cache.ContainsKey<T>(key))
        {
            var value = await constructor();
            cache.Add(category, key, value);

            return value;
        }

        return cache.GetValue<T>(key);
    }
}