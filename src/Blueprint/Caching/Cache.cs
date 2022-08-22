using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blueprint.Caching.Configuration;
using Microsoft.Extensions.Logging;

namespace Blueprint.Caching;

/// <summary>
/// Provides an entry point into the Blueprint caching subsystem.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Commonly used name, not changing.")]
public class Cache : ICache
{
    /// <summary>
    /// An implementation of <see cref="ICache"/> that does nothing, to be used in
    /// test scenarios.
    /// </summary>
    public static readonly ICache NoCache = new NoCache();

    private readonly bool _enabled;
    private readonly IEnumerable<ICachingStrategy> _orderedStrategies;
    private readonly ILogger<Cache> _logger;

    /// <summary>
    /// Initializes a new instance of the Cache class.
    /// </summary>
    /// <param name="cacheProviders">The registered list of cache providers, from which one will be picked based on current configuration.</param>
    /// <param name="logger">Logger to use.</param>
    public Cache(IEnumerable<ICacheProvider> cacheProviders, ILogger<Cache> logger)
    {
        this._logger = logger;
        var cachingConfiguration = CachingConfiguration.Current;

        logger.LogInformation("Caching enabled = {0}.", cachingConfiguration.IsEnabled);
        logger.LogInformation("Found {0} caching strategies.", cachingConfiguration.Strategies.Count);
        logger.LogInformation("Using caching provider {0}.", cachingConfiguration.ProviderType);

        if (cachingConfiguration.ProviderType == null)
        {
            throw new InvalidOperationException("Cannot create a Cache without specifing ProviderType");
        }

        this._orderedStrategies = cachingConfiguration.Strategies.OrderBy(s => s.Priority).ToArray();
        this._enabled = cachingConfiguration.IsEnabled;

        this.Provider = cacheProviders.SingleOrDefault(c => c.GetType() == cachingConfiguration.ProviderType);
    }

    /// <summary>
    /// Gets the provider of this cache, which may be null should a provider type not be specified.
    /// </summary>
    public ICacheProvider Provider { get; }

    /// <summary>
    /// Adds a value to this cache using the specified unique key.
    /// </summary>
    /// <param name="category">
    /// A category the value belongs to, used to specify 'profiles' of caching via.
    /// configuration such as 'Content', or 'Reports'.
    /// </param>
    /// <param name="key">
    /// The unique key of this cache item.
    /// </param>
    /// <param name="value">
    /// The value to be stored for the given key.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted, inferred by the compiler.
    /// </typeparam>
    public void Add<T>(string category, object key, T value)
    {
        if (this._enabled)
        {
            this._logger.LogTrace("Attempting to add a new entry to the cache. category={0}  key={1}.", category, key);

            var strategy = this._orderedStrategies.FirstOrDefault(s => s.AppliesTo(category, value));

            if (strategy == null)
            {
                this._logger.LogDebug("No strategy found, the item will not be added to the cache.");
                return;
            }

            var options = strategy.GetOptions();

            if (ShouldInsertIntoCache(options))
            {
                this._logger.LogDebug("Adding item to cache. key={0} options={1}", key, options);

                if (value == null)
                {
                    this.Provider.Add(GenerateStorageKey<T>(key), NullValue<T>.Instance, options);
                }
                else
                {
                    this.Provider.Add(GenerateStorageKey<T>(key), value, options);
                }
            }
        }
    }

    /// <summary>
    /// Returns a value that indicates whether ot not the cache containers the given key.
    /// </summary>
    /// <param name="key">
    /// The key to check for existence.
    /// </param>
    /// <returns>
    /// Whether or not this cache contains the specified key.
    /// </returns>
    /// <typeparam name="T">
    /// The type of the value being inserted, inferred by the compiler.
    /// </typeparam>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to avoid key naming conflicts")]
    public bool ContainsKey<T>(object key)
    {
        return this._enabled && this.Provider.ContainsKey(GenerateStorageKey<T>(key));
    }

    /// <summary>
    /// Gets the value that has been stored for the given key, or <c>null</c> if nothing
    /// has bene stored in this cache for the given key.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value that has been stored for the given key.
    /// </typeparam>
    /// <param name="key">
    /// The unique key that represents the cache item to retrieve.
    /// </param>
    /// <returns>
    /// The value that has bene stored in this cache for the given key, or <c>null</c>.
    /// </returns>
    public T GetValue<T>(object key)
    {
        if (this._enabled)
        {
            var storedItem = this.Provider.GetValue(GenerateStorageKey<T>(key));

            if (storedItem is NullValue<T>)
            {
                this._logger.LogTrace("Value in cache saved as null. key={0}", key);
                return default;
            }

            if (storedItem != null)
            {
                this._logger.LogTrace("Cache hit. key={0} value_type={1}", key, storedItem);
            }
            else
            {
                this._logger.LogTrace("Cache miss. key={0}", key);
            }

            return (T)storedItem;
        }

        return default;
    }

    /// <summary>
    /// Removes the cache item with the specified key, performing no action
    /// if the key does not exist.
    /// </summary>
    /// <param name="key">
    /// The key of the cache item that should be removed.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value being inserted, inferred by the compiler.
    /// </typeparam>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to avoid key naming conflicts")]
    public void Remove<T>(object key)
    {
        if (this._enabled)
        {
            this.Provider.Remove(GenerateStorageKey<T>(key));
        }
    }

    /// <summary>
    /// When a value is stored within the cache the key will be generated from the type of object being
    /// stored plus the key being passed into the various functions, to help avoid collisions. This method
    /// will generate that unique key.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being stored, checked or removed.
    /// </typeparam>
    /// <param name="key">
    /// The client key.
    /// </param>
    /// <returns>
    /// A string key to be used as the unique key in the backing cache.
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to avoid key naming conflicts")]
    private static string GenerateStorageKey<T>(object key)
    {
        return key + " of " + typeof(T).Name;
    }

    private static bool ShouldInsertIntoCache(CacheOptions options)
    {
        return options != null && options != CacheOptions.NotCached;
    }

    private class NullValue<T>
    {
        public static readonly NullValue<T> Instance = new NullValue<T>();
    }
}