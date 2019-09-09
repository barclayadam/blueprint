using Blueprint.Api.Validation;

namespace Blueprint.Tests.Core.Validation.RequiredIfNotAttribute_Tests
{
    using System.Threading.Tasks;

    using Blueprint.Core.Validation;

    using NUnit.Framework;

    public class Given_String_Property_RequiredIfNot_Other_String_Property_Is_One_Of_Two_Specified_Values_Where_One_Is_Null
    {
        public class Validatable
        {
            public string PropertyToCheckAgainst { get; set; }

            [RequiredIfNot("PropertyToCheckAgainst", new[] { "Not Required", null })]
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
