﻿namespace Blueprint.Tests.Core.Validation.Validator_Tests
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    using Blueprint.Core.Validation;

    using FluentAssertions;

    using NUnit.Framework;

    public class Given_Thing_With_DataAnnotation_Validation_Attributes
    {
        private BlueprintValidator validator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });
        }

        [Test]
        public async Task When_Invoked_Then_Throws_MessageValidationException_With_Correct_Number_Of_ValidationResults()
        {
            // Arrange
            var invalidMessage = new MessageWithDataAnnotationAttributes { RequiredStringProperty = null };

            // Act
            var results = await validator.GetValidationResultsAsync(invalidMessage, null);

            // Assert
            results.Count.Should().Be(1);
        }

        [Test]
        public async Task When_Invoked_Then_Throws_MessageValidationException_With_ValidationResults_Containing_Correct_Property_Name()
        {
            // Arrange
            var invalidMessage = new MessageWithDataAnnotationAttributes { RequiredStringProperty = null };

            // Act
            var results = await validator.GetValidationResultsAsync(invalidMessage, null);

            // Assert
            results.AsDictionary().Keys.Single().Should().Be(nameof(MessageWithDataAnnotationAttributes.RequiredStringProperty));
        }

        private class MessageWithDataAnnotationAttributes
        {
            [Required]
            public string RequiredStringProperty { get; set; }
        }
    }
}