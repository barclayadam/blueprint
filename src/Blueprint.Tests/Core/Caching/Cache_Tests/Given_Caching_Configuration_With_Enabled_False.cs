using Blueprint.Core.Caching;
using Blueprint.Core.Caching.Configuration;
using Blueprint.Testing;
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
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                  </configSections>

                  <caching enabled='false' provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                    <rules>
                        <fixed type='*' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                    </rules>
                  </caching>");

            // Act
            CachingConfiguration.Current = (CachingConfiguration)configFile.GetSection("caching");
        }

        [Test]
        public void When_Item_Added_Then_ContainsKey_Is_False()
        {
            // Arrange
            var cache = new Cache(new Container());
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
            var cache = new Cache(new Container());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            cache.GetValue<string>("My Key").Should().BeNull();
        }

        [Test]
        public void When_Item_Added_Then_Value_Not_Added_To_Provider()
        {
            // Arrange
            var cache = new Cache(new Container());

            // Act
            cache.Add("Default", "My Key", "My Value");

            // Assert
            ((FakeCacheProvider)cache.Provider).CachedValues.Should().BeEmpty();
        }

        [Test]
        public void When_Key_Removed_Then_Nothing_Happens()
        {
            // Arrange
            var cache = new Cache(new Container());
            cache.Add("Default", "My Key", "My Value");

            // Act
            cache.Remove<string>("My Key");

            // Assert
            Assert.Pass("Test was successful, should be able top always successfully call Remove");
        }
    }
}