using System;

using Blueprint.Core.Caching;
using Blueprint.Core.Caching.Configuration;
using Blueprint.Tests.Fakes;
using FluentAssertions;
using NUnit.Framework;
using StructureMap;

namespace Blueprint.Tests.Core.Caching.Cache_Tests
{
    public class Given_Caching_Configuration_With_Enabled_False
    {
        [SetUp]
        public void CreateConfiguration()
        {
            // Act
            CachingConfiguration.Current = new CachingConfiguration
            {
                IsEnabled = false,
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
        public void When_Item_Added_Then_ContainsKey_Is_False()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() });
            cache.Add("Default", "My Key", "My Value");

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.ContainsKey<string>("My Key").Should().BeFalse();
        }

        [Test]
        public void When_Item_Added_Then_GetValue_Returns_Default_Value()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() });

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.GetValue<string>("My Key").Should().BeNull();
        }

        [Test]
        public void When_Item_Added_Then_Value_Not_Added_To_Provider()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() });

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            ((FakeCacheProvider)cache.Provider).CachedValues.Should().BeEmpty();
        }

        [Test]
        public void When_Key_Removed_Then_Nothing_Happens()
        {
            // Arrange
            var cache = new Cache(new ICacheProvider[] { new FakeCacheProvider() });
            cache.Add("Default", "My Key", "My Value");

            // Act
            cache.Remove<string>("My Key");

            // Assert
            Assert.Pass("Test was successful, should be able top always successfully call Remove");
        }
    }
}
