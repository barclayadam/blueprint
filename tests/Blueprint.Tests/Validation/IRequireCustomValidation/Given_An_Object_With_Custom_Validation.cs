using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.IRequireCustomValidation;

public class Given_An_Object_With_Custom_Validation
{
    public class ObjectWithNoFailures : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return new List<ValidationResult>();
        }
    }

    public class ObjectWithOneFailure : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return new List<ValidationResult> { new ValidationResult("First Message") };
        }
    }

    public class ObjectWithTwoFailures : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return new List<ValidationResult> { new ValidationResult("First Message"), new ValidationResult("Second Message") };
        }
    }

    [Test]
    public async Task When_Object_Returns_No_Validation_Failures_Then_GetFailures_Returns_Empty_Collection()
    {
        // Arrange
        var validatedObject = new ObjectWithNoFailures();
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatedObject, null);

        // Assert
        failures.Count.Should().Be(0);
    }

    [Test]
    public async Task When_Object_Returns_One_Validation_Failure_Then_GetFailures_Returns_Collection_With_One_Result()
    {
        // Arrange
        var validatedObject = new ObjectWithOneFailure();
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatedObject, null);

        // Assert
        failures.Count.Should().BeGreaterThan(0);
        failures.AsDictionary().First().Value.Should().Contain("First Message");
    }

    [Test]
    public async Task When_Object_Returns_Two_Validation_Failures_Then_GetFailures_Returns_Collection_With_Two_Results()
    {
        // Arrange
        var validatedObject = new ObjectWithTwoFailures();
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatedObject, null);

        // Assert
        failures.Count.Should().BeGreaterThan(0);
        failures.AsDictionary()[ValidationFailures.FormLevelPropertyName].Should().Contain("First Message");
        failures.AsDictionary()[ValidationFailures.FormLevelPropertyName].Should().Contain("Second Message");
    }
}