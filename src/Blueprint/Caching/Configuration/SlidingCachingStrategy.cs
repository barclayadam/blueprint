﻿using System;

namespace Blueprint.Caching.Configuration;

/// <summary>
/// A caching element that represents a sliding expiration, such that an item will expire a certain time
/// after its <strong>Last</strong> access.
/// </summary>
public class SlidingCachingStrategy : CachingStrategy
{
    /// <summary>
    /// Gets or sets the time span that determines for how long an item stays in the cache after its
    /// last access.
    /// </summary>
    public TimeSpan TimeSpan { get; set; }

    /// <summary>
    /// Gets the options for the specified value, which will be a sliding cache
    /// options instance.
    /// </summary>
    /// <returns>
    /// The cache options to use when storing the specified value.
    /// </returns>
    /// <seealso cref="CacheOptions.Sliding"/>
    public override CacheOptions GetOptions()
    {
        return CacheOptions.Sliding(this.ItemPriority, this.TimeSpan);
    }
}