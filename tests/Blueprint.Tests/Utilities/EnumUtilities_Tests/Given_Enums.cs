using System;
using System.Collections.Generic;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.EnumUtilities_Tests
{
    public class Given_Enums
    {
        private enum TestEnum
        {
            NonDescribedValue,

            [System.ComponentModel.Description("DifferentValue")]
            DescribedValue
        }

        [Test]
        public void When_Getting_Description_Then_Success()
        {
            TestEnum.NonDescribedValue.GetDescription().Should().Be("NonDescribedValue");
            TestEnum.DescribedValue.GetDescription().Should().Be("DifferentValue");
        }

        [Test]
        public void When_Getting_Descriptions_Then_Success()
        {
            EnumUtilities
                .GetDescriptions(typeof(TestEnum))
                .Should()
                .Contain(new[]
                {
                    "NonDescribedValue",
                    "DifferentValue",
                });
        }

        [Test]
        public void When_Getting_Descriptions_With_Keys_Then_Success()
        {
            EnumUtilities
                .GetDescriptionsWithKeys(typeof(TestEnum))
                .Should()
                .Contain(
                    new KeyValuePair<string, string>("NonDescribedValue", "NonDescribedValue"),
                    new KeyValuePair<string, string>("DescribedValue", "DifferentValue"));
        }

        [Test]
        public void When_Getting_Enum_From_Description_Then_Success()
        {
            EnumUtilities.FromDescription<TestEnum>("NonDescribedValue").Should().Be(TestEnum.NonDescribedValue);

            // Supports both the actual name of enum + [Description]
            EnumUtilities.FromDescription<TestEnum>("DescribedValue").Should().Be(TestEnum.DescribedValue);
            EnumUtilities.FromDescription<TestEnum>("DifferentValue").Should().Be(TestEnum.DescribedValue);
        }

        [Test]
        public void When_Getting_Enum_With_Strict_Casing_Then_Success()
        {
            // Ordinal should mean case-sensitive check
            EnumUtilities.TryFromDescription<TestEnum>("NonDescribedVALUE", StringComparison.Ordinal, out _).Should().BeFalse();
            EnumUtilities.TryFromDescription<TestEnum>("DescribedVALUE", StringComparison.Ordinal, out _).Should().BeFalse();
            EnumUtilities.TryFromDescription<TestEnum>("DifferentVALUE", StringComparison.Ordinal, out _).Should().BeFalse();
        }
    }
}
