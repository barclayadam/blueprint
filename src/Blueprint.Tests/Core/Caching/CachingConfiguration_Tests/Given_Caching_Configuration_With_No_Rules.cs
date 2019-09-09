using Blueprint.Core.Caching.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.CachingConfiguration_Tests
{
    public class Given_Caching_Configuration_With_No_Rules
    {
        [Test]
        public void When_Enabled_Attribute_Is_False_Then_IsEnabled_Should_Be_False()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching enabled='false' />");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void When_Enabled_Attribute_Is_True_Then_IsEnabled_Should_Be_True()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching enabled='true' provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests' />");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void When_Getting_Rules_Count_Then_Answer_Should_Be_Zero()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching />");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Count.Should().Be(0);
        }
    }
}