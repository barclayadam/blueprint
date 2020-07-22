using System;
using Blueprint.Caching;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Caching.NoCacheProvider_Tests
{
    public class Given_NoCacheProvider_Instance
    {
        [Test]
        public void When_Item_Added_Then_ContainsKey_Is_False()
        {
            // Arrange
            var cache = new NoCacheProvider();
            cache.Add("My Key", "My Value", CacheOptions.Absolute(CacheItemPriority.Medium, TimeSpan.FromDays(1)));

            // Act
            var result = cache.ContainsKey("My Key");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void When_Item_Added_Then_GetValue_Returns_Default_Value()
        {
            // Arrange
            var cache = new NoCacheProvider();
            cache.Add("My Key", "My Value", CacheOptions.Absolute(CacheItemPriority.Medium, TimeSpan.FromDays(1)));

            // Act
            var result = cache.GetValue("My Key");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_Key_Removed_Then_Nothing_Happens()
        {
            // Arrange
            var cache = new NoCacheProvider();
            cache.Add("My Key", "My Value", CacheOptions.Absolute(CacheItemPriority.Medium, TimeSpan.FromDays(1)));

            // Act
            cache.Remove("My Key");

            // Assert
            Assert.Pass("Test was successful, should be able to always successfully call Remove");
        }
    }
}
