﻿using Blueprint.Core.Caching;
using Blueprint.Core.Caching.Configuration;
using Blueprint.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.Cache_Tests
{
    public class Given_Caching_Configuration_With_No_Strategies
    {
        [SetUp]
        public void CreateConfiguration()
        {
            CachingConfiguration.Current = new CachingConfiguration
            {
                IsEnabled = true,
                ProviderType = typeof(FakeCacheProvider)
            };
        }

        [Test]
        public void When_Item_Added_Then_ContainsKey_Is_False()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.ContainsKey<string>("My Key").Should().BeFalse();
        }

        [Test]
        public void When_Item_Added_Then_GetValue_Returns_Default_Value()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.GetValue<string>("My Key").Should().BeNull();
        }

        [Test]
        public void When_Item_Added_Then_Value_Not_Added_To_Provider()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            ((FakeCacheProvider)cache.Provider).CachedValues.Should().BeEmpty();
        }

        [Test]
        public void When_Key_Removed_Then_Nothing_Happens()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());
            cache.Add("Default", "My Key", "My Value");

            // Act
            cache.Remove<string>("My Key");

            // Assert
            Assert.Pass("Test was successful, should be able top always successfully call Remove");
        }
    }
}
