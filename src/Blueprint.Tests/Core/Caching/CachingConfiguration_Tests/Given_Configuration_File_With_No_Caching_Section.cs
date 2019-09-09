using Blueprint.Core.Caching.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.CachingConfiguration_Tests
{
    public class Given_Configuration_File_With_Empty_Caching_Element
    {
        [Test]
        public void When_Getting_Configuration_Section_Then_Enabled_Will_Default_To_True()
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
            configurationSection.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void When_Getting_Configuration_Section_Then_No_Strategies_Will_Be_Defined()
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