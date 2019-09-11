using System.Threading.Tasks;
using Blueprint.Api.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Validation.RequiredIfNotAttribute_Tests
{
    public class Given_String_Property_RequiredIfNot_Other_String_Property_Is_Null_Value
    {
        public class Validatable
        {
            public string PropertyToCheckAgainst { get; set; }

            [RequiredIfNot("PropertyToCheckAgainst", null)]
            public string ConditionallyRequiredProperty { get; set; }
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Triggeres_Required_Validation_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Required", ConditionallyRequiredProperty = "Populated" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Triggeres_Required_Validation_And_ConditionallyRequiredProperty_Is_Empty_Then_Invalid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Required", ConditionallyRequiredProperty = null };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.Contains(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Does_Not_Trigger_Required_Validation_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = null, ConditionallyRequiredProperty = "Populated" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Does_Not_Trigger_Required_Validation_And_ConditionallyRequiredProperty_Is_Empty_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = null, ConditionallyRequiredProperty = null };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }
    }
}
