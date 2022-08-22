using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.Validator_Tests;

public class Given_Message_With_Required_Nullable_Value_Type_Property
{
    private BlueprintValidator validator;
    private ApiOperationContext apiOperationContext;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });
        apiOperationContext = null;
    }

    [Test]
    public async Task When_Invoked_With_Null_Property_Value_Then_Fails_Validation()
    {
        // Arrange
        var invalidMessage = new MessageWithNullabeProperty { RequiredIntProperty = null };

        // Act
        var result = await validator.GetValidationResultsAsync(invalidMessage, apiOperationContext);

        // Assert
        result.Count.Should().Be(1);
    }

    [Test]
    public async Task  When_Invoked_With_Value_Then_Passes_Validation()
    {
        // Arrange
        var validMessage = new MessageWithNullabeProperty { RequiredIntProperty = 666 };

        // Act
        var result = await validator.GetValidationResultsAsync(validMessage, apiOperationContext);

        // Assert
        result.Count.Should().Be(0);
    }

    private class MessageWithNullabeProperty
    {
        [Required]
        public int? RequiredIntProperty { get; set; }
    }
}