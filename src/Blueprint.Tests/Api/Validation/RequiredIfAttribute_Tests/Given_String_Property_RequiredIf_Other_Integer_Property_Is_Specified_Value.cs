using System.Threading.Tasks;
using Blueprint.Api.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Validation.RequiredIfAttribute_Tests
{
    public class Given_String_Property_RequiredIf_Other_Integer_Property_Is_Specified_Value
    {
        public class Validatable
        {
            public int PropertyToCheckAgainst { get; set; }

            [RequiredIf("PropertyToCheckAgainst", 1)]
            public string ConditionallyRequiredProperty { get; set; }
        }

        [Test]
        public async Task When_PropertyToCheckAgainst_Has_Correct_Value_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable { PropertyToCheckAgainst = 1, ConditionallyRequiredProperty = "Populated" };
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
            var validatable = new Validatable { PropertyToCheckAgainst = 1, ConditionallyRequiredProperty = "" };
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
            var validatable = new Validatable { PropertyToCheckAgainst = 1, ConditionallyRequiredProperty = null };
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
            var validatable = new Validatable { PropertyToCheckAgainst = 0, ConditionallyRequiredProperty = "" };
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
            var validatable = new Validatable { PropertyToCheckAgainst = 0, ConditionallyRequiredProperty = null };
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
            var validatable = new Validatable { PropertyToCheckAgainst = 0, ConditionallyRequiredProperty = "Populated" };
            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
        }
    }
}
