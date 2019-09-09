using Blueprint.Api.Validation;

namespace Blueprint.Tests.Core.Validation.RequiredIfNotAttribute_Tests
{
    using System.Threading.Tasks;

    using Blueprint.Core.Validation;

    using NUnit.Framework;

    public class Given_String_Property_RequiredIfNot_Other_String_Property_Is_Specified_Value
    {
        public class Validatable
        {
            public string PropertyToCheckAgainst { get; set; }

            [RequiredIfNot("PropertyToCheckAgainst", "Not Required")]
            public string ConditionallyRequiredProperty { get; set; }
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Has_Correct_Value_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
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
        public async Task When_PropertyToCheckAgainst_Has_Correct_Value_And_ConditionallyRequiredProperty_Is_Blank_Then_Invalid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Required", ConditionallyRequiredProperty = "" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.Contains(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Has_Correct_Value_And_ConditionallyRequiredProperty_Is_Null_Then_Invalid()
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
        public async Task When_PropertyToCheckAgainst_Has_Value_That_Does_Not_Force_ConditionallyRequiredProperty_To_Be_Required_And_ConditionallyRequiredProperty_Is_Blank_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Not Required", ConditionallyRequiredProperty = "" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Has_Value_That_Does_Not_Force_ConditionallyRequiredProperty_To_Be_Required_And_ConditionallyRequiredProperty_Is_Null_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Not Required", ConditionallyRequiredProperty = null };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Has_Value_That_Does_Not_Force_ConditionallyRequiredProperty_To_Be_Required_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = "Not Required", ConditionallyRequiredProperty = "Populated" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }
    }
}
