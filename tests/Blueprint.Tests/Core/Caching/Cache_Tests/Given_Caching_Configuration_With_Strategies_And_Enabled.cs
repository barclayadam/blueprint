using System;

using Blueprint.Caching;
using Blueprint.Caching.Configuration;
using Blueprint.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.Cache_Tests
{
    public class Given_Caching_Configuration_With_Strategies_And_Enabled
    {
        [SetUp]
        public void CreateConfiguration()
        {
            CachingConfiguration.Current = new CachingConfiguration
            {
                IsEnabled = true,
                ProviderType = typeof(FakeCacheProvider),
                Strategies =
                {
                    new FixedCachingStrategy
                    {
                        TypeName = "*",
                        TimeSpan = TimeSpan.FromMinutes(5),
                        ItemPriority = CacheItemPriority.Low,
                        Priority = -1000
                    }
                }
            };
        }

        [Test]
        public void When_Item_Added_Then_ContainsKey_Is_True()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.ContainsKey<string>("My Key").Should().BeTrue();
        }

        [Test]
        public void When_Item_Added_Then_GetValue_Returns_Added_Value()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.GetValue<string>("My Key").Should().Be("My Value");
        }

        [Test]
        public void When_Item_Added_Then_Value_Added_To_Provider()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            ((FakeCacheProvider)cache.Provider).GetItems<string>().Should().Contain("My Value");
        }

        [Test]
        public void When_Key_Removed_Then_Item_Removed()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());
            cache.Add("Default", "My Key", "My Value");

            // Act
            cache.Remove<string>("My Key");

            // Assert
            cache.GetValue<string>("My Key").Should().BeNull();
        }

        [Test]
        public void When_Null_Item_Added_Then_Able_To_Retrieve_Null()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add<string>("Default", "My Key", null);

            // Assert
            cache.GetValue<string>("My Key").Should().BeNull();
        }

        [Test]
        public void When_Null_Item_Added_Then_Value_Added_To_Provider()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() }, new NullLogger<Cache>());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.ContainsKey<string>("My Key").Should().BeTrue();
        }
    }
}
