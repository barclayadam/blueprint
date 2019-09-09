using System.Linq;
using Blueprint.Core;
using Blueprint.Core.Caching.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.CachingConfiguration_Tests
{
    public class Given_Caching_Configuration_With_Fixed_Rule : With_Caching_Element
    {
        protected override string CachingElementName { get { return "fixed"; } }

        [TearDown]
        public void ResetSystemTime()
        {
            SystemTime.RestoreDefault();
        }

        [Test]
        public void When_Getting_Option_From_Strategy_Then_It_Should_Have_A_Fixed_Expiration()
        {
            // Arrange
            using (SystemTime.PlayTimelord())
            {
                var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                        @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <fixed type='object' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

                // Act
                var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

                // Assert
                configurationSection.Rules.Count.Should().Be(1);
                configurationSection.Rules.Strategies.Single().GetOptions().AbsoluteExpiration.Should().Be(
                    SystemTime.UtcNow.AddMinutes(5));

            }
        }
    }
}