﻿using System.Threading.Tasks;
using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.RequiredIfAttribute_Tests;

public class Given_String_Property_RequiredIf_Other_String_Property_Is_One_Of_Two_Specified_Values
{
    public class Validatable
    {
        public string PropertyToCheckAgainst { get; set; }

        [RequiredIf("PropertyToCheckAgainst", new [] { "Required", "Another Required" })]
        public string ConditionallyRequiredProperty { get; set; }
    }

    [Test]
    public async Task When_PropertyToCheckAgainst_Has_First_Correct_Value_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
    {
        // Arrange
        var validatable = new Given_String_Property_RequiredIf_Other_String_Property_Is_Specified_Value.Validatable { PropertyToCheckAgainst = "Required", ConditionallyRequiredProperty = "Populated" };
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatable, null);
        var failedProperties = failures.AsDictionary().Keys;

        // Assert
        CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
    }

    [Test]
    public async Task When_PropertyToCheckAgainst_Has_Second_Correct_Value_And_ConditionallyRequiredProperty_Is_Populated_Then_Valid()
    {
        // Arrange
        var validatable = new Given_String_Property_RequiredIf_Other_String_Property_Is_Specified_Value.Validatable { PropertyToCheckAgainst = "Another Required", ConditionallyRequiredProperty = "Populated" };
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatable, null);
        var failedProperties = failures.AsDictionary().Keys;

        // Assert
        CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
    }

    [Test]
    public async Task When_PropertyToCheckAgainst_Has_First_Correct_Value_And_ConditionallyRequiredProperty_Is_Blank_Then_Invalid()
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
    public async Task When_PropertyToCheckAgainst_Has_Second_Correct_Value_And_ConditionallyRequiredProperty_Is_Blank_Then_Invalid()
    {
        // Arrange
        var validatable = new Validatable { PropertyToCheckAgainst = "Another Required", ConditionallyRequiredProperty = "" };
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatable, null);
        var failedProperties = failures.AsDictionary().Keys;

        // Assert
        CollectionAssert.Contains(failedProperties, "ConditionallyRequiredProperty");
    }

    [Test]
    public async Task When_PropertyToCheckAgainst_Has_First_Correct_Value_And_ConditionallyRequiredProperty_Is_Null_Then_Invalid()
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
    public async Task When_PropertyToCheckAgainst_Has_Second_Correct_Value_And_ConditionallyRequiredProperty_Is_Null_Then_Invalid()
    {
        // Arrange
        var validatable = new Validatable { PropertyToCheckAgainst = "Another Required", ConditionallyRequiredProperty = null };
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
        var validatable = new Validatable { PropertyToCheckAgainst = "NotRequired", ConditionallyRequiredProperty = "" };
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
        var validatable = new Validatable { PropertyToCheckAgainst = "NotRequired", ConditionallyRequiredProperty = null };
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
        var validatable = new Validatable { PropertyToCheckAgainst = "NotRequired", ConditionallyRequiredProperty = "Populated" };
        var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

        // Act
        var failures = await validator.GetValidationResultsAsync(validatable, null);
        var failedProperties = failures.AsDictionary().Keys;

        // Assert
        CollectionAssert.DoesNotContain(failedProperties, "ConditionallyRequiredProperty");
    }
}