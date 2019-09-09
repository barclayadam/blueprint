using System.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.CachingConfiguration_Tests
{
    public class Given_Caching_Configuration_With_Unsupported_Rule
    {
        [Test]
        public void When_Getting_Configuration_Section_Then_ConfigurationErrorsException_Thrown()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                   @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <unsupported type='object' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var exception = Assert.Catch<ConfigurationErrorsException>(() => configFile.GetSection("caching"));

            // Assert
            exception.Should().NotBeNull();
        }
    }
}