using System;
using System.Linq;
using Blueprint.Core.Caching;
using Blueprint.Core.Caching.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Caching.CachingConfiguration_Tests
{
    public abstract class With_Caching_Element
    {
        protected abstract string CachingElementName { get; }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Category_Name_Does_Not_Match_Then_AppliesTo_Is_False(Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" category='A Category' type='System.DateTime' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                 "A Different Category",
                                                                                 Activator.CreateInstance(typeToCache)).Should().BeFalse(
                        );
        }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Category_Name_Matches_And_Type_Does_Not_Then_AppliesTo_Is_False(Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" category='A Category' type='System.DateTime' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                 "A Category",
                                                                                 Activator.CreateInstance(typeToCache)).Should().BeFalse(
                        );
        }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Category_Name_Matches_With_Different_Case_With_No_Type_Specified_Then_AppliesTo_Is_True(
                Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" category='A Category' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                 "A CATEGORY",
                                                                                 Activator.CreateInstance(typeToCache)).Should().BeTrue(
                        );
        }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Category_Name_Matches_With_No_Type_Specified_Then_AppliesTo_True(Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" category='A Category' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                 "A Category",
                                                                                 Activator.CreateInstance(typeToCache)).Should().BeTrue(
                        );
        }

        [Test]
        public void When_Getting_Option_From_Strategy_Then_It_Should_Have_The_Correct_Priority()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" type='object' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().GetOptions().Priority.Should().Be(
                        CacheItemPriority.Low);
        }

        [Test]
        public void
                When_Getting_Option_From_Strategy_Then_It_Should_Have_The_Correct_Strategy_Priority_From_Rule_Priority()
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <"
                                                               + CachingElementName
                                                               +
                                                               @" type='object' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().Priority.Should().Be(-1000);
        }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Items_Category_Name_Is_Null_And_Type_Matches_Then_AppliesTo_Is_True(Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                        <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                    </configSections>

                    <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                        <rules>
                            <" + CachingElementName + @" type='System.DateTime' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                        </rules>
                    </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(null, new DateTime()).Should().BeTrue();
        }

        [Test]
        [TestCase(typeof(object), "System.Object")]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule),
                "Blueprint.Tests.Core.caching.CachingConfiguration_Tests.Given_Caching_Configuration_With_Fixed_Rule")]
        [TestCase(typeof(Given_Caching_Configuration_With_No_Rules),
                "Blueprint.Tests.Core.caching.CachingConfiguration_Tests.Given_Caching_Configuration_With_No_Rules")]
        [TestCase(typeof(Given_Caching_Configuration_With_Sliding_Rule),
                "blueprint.tests.core.caching.cachingConfiguration_tests.given_caching_configuration_with_sliding_rule")]
        public void When_Type_Defined_As_Fully_Named_Concrete_Type_Then_Applies_To_Should_Be_True_When_Given_Instance_Of_That_Type(
                Type typeToCache, string typeName)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                                                                <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                                                            </configSections>

                                                            <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                                                                <rules>
                                                                    <" + CachingElementName + @" type='" + typeName + @"' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                                                                </rules>
                                                            </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                 "Default",
                                                                                 Activator.CreateInstance(typeToCache)).Should().BeTrue();
        }

        [Test]
        [TestCase(typeof(object), "Object")]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule),
                "Given_Caching_Configuration_With_Fixed_Rule")]
        [TestCase(typeof(Given_Caching_Configuration_With_No_Rules),
                "CachingConfiguration_Tests.Given_Caching_Configuration_With_No_Rules")]
        [TestCase(typeof(Given_Caching_Configuration_With_Sliding_Rule),
                "given_caching_configuration_with_sliding_rule")]
        public void When_Type_Defined_As_Partial_Ends_With_Named_Concrete_Type_Then_Applies_To_Should_Be_True_When_Given_Instance_Of_That_Type(
                Type typeToCache, string typeName)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                                                                <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                                                            </configSections>

                                                            <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                                                                <rules>
                                                                    <" + CachingElementName + @" type='" + typeName + @"' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                                                                </rules>
                                                            </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            var appliesTo = configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                     "Default",
                                                                                     Activator.CreateInstance(typeToCache));
            appliesTo.Should().BeTrue();
        }

        [Test]
        [TestCase(typeof(object))]
        [TestCase(typeof(Given_Caching_Configuration_With_Fixed_Rule))]
        public void When_Type_Is_Asterisk_Then_Applies_To_Should_Always_Be_True(Type typeToCache)
        {
            // Arrange
            var configFile =
                    ConfigCreator.CreateTemporaryConfiguration(
                                                               @"<configSections>
                                                                <section name='caching' type='Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core' />
                                                            </configSections>

                                                            <caching provider='Blueprint.Tests.Fakes.FakeCacheProvider, Blueprint.Tests'>
                                                                <rules>
                                                                    <"
                                                               + CachingElementName
                                                               +
                                                               @" type='*' timeSpan='00:05:00' itemPriority='Low' rulePriority='-1000' />
                                                                </rules>
                                                            </caching>");

            // Act
            var configurationSection = (CachingConfiguration)configFile.GetSection("caching");

            // Assert
            var appliesTo = configurationSection.Rules.Strategies.Single().AppliesTo(
                                                                                     "Default",
                                                                                     Activator.CreateInstance(typeToCache));
            appliesTo.Should().BeTrue();
        }
    }
}