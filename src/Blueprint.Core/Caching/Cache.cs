using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blueprint.Core.Caching.Configuration;
using NLog;
using StructureMap;

namespace Blueprint.Core.Caching
{
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

        private static readonly Logger Log = LogManager.GetLogger("Blueprint.Caching");

        private readonly ICacheProvider cacheProvider;
        private readonly bool enabled;
        private readonly IEnumerable<ICachingStrategy> orderedStrategies;

        /// <summary>
        /// Initializes a new instance of the Cache class.
        /// </summary>
        public Cache(IContainer container)
        {
            var cachingConfiguration = CachingConfiguration.Current;

            Log.Info("Caching enabled = {0}.", cachingConfiguration.IsEnabled);
            Log.Info("Found {0} caching strategies.", cachingConfiguration.Strategies.Count);
            Log.Info("Using caching provider {0}.", cachingConfiguration.ProviderType);

            if (cachingConfiguration.ProviderType == null)
            {
                throw new InvalidOperationException("Cannot create a Cache without specifing ProviderType");
            }

            orderedStrategies = cachingConfiguration.Strategies.OrderBy(s => s.Priority).ToArray();
            enabled = cachingConfiguration.IsEnabled;
            cacheProvider = (ICacheProvider)container.GetInstance(cachingConfiguration.ProviderType);
        }

        /// <summary>
        /// Gets the provider of this cache, which may be null should a provider type not be specified.
        /// </summary>
        public ICacheProvider Provider => cacheProvider;

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
            if (enabled)
            {
                Log.Trace("Attempting to add a new entry to the cache. category={0}  key={1}.", category, key);

                var strategy = orderedStrategies.FirstOrDefault(s => s.AppliesTo(category, value));

                if (strategy == null)
                {
                    Log.Debug("No strategy found, the item will not be added to the cache.");
                    return;
                }

                var options = strategy.GetOptions();

                if (ShouldInsertIntoCache(options))
                {
                    Log.Debug("Adding item to cache. key={0} options={1}", key, options);

                    if (value == null)
                    {
                        cacheProvider.Add(GenerateStorageKey<T>(key), NullValue<T>.Instance, options);
                    }
                    else
                    {
                        cacheProvider.Add(GenerateStorageKey<T>(key), value, options);
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
                Justification = "Type is used to avoid key naming conflicts")]
        public bool ContainsKey<T>(object key)
        {
            return enabled && cacheProvider.ContainsKey(GenerateStorageKey<T>(key));
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
            if (enabled)
            {
                var storedItem = cacheProvider.GetValue(GenerateStorageKey<T>(key));

                if (storedItem is NullValue<T>)
                {
                    Log.Trace("Value in cache saved as null. key={0}", key);
                    return default(T);
                }

                if (storedItem != null)
                {
                    Log.Trace("Cache hit. key={0} value_type={1}", key, storedItem);
                }
                else
                {
                    Log.Trace("Cache miss. key={0}", key);
                }

                return (T)storedItem;
            }

            return default(T);
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
                Justification = "Type is used to avoid key naming conflicts")]
        public void Remove<T>(object key)
        {
            if (enabled)
            {
                cacheProvider.Remove(GenerateStorageKey<T>(key));
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
                Justification = "Type is used to avoid key naming conflicts")]
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
}
