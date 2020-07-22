using System;
using System.Threading.Tasks;
using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.GreaterThanPropertyAttribute_Tests
{
    public class Given_DateTime_Value
    {
        public class Validatable
        {
            public DateTime? PropertyToCheckAgainst { get; set; }

            [GreaterThanProperty("PropertyToCheckAgainst")]
            public DateTime? MustBeGreaterThanProperty { get; set; }
        }

        [Test]
        public async Task When_Value_Is_Equal_To_PropertyToCheck_Value_Then_InValid()
        {
            var date = DateTime.Now;

            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = date,
                MustBeGreaterThanProperty = date
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.Contains(failedProperties, "MustBeGreaterThanProperty");
        }

        [Test]
        public async Task When_Value_Is_Greater_Than_PropertyToCheck_Value_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = DateTime.Now,
                MustBeGreaterThanProperty = DateTime.Now.AddDays(1)
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "MustBeGreaterThanProperty");
        }

        public async Task When_Value_Is_Less_Than_PropertyToCheck_Value_Then_InValid()
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = DateTime.Now,
                MustBeGreaterThanProperty = DateTime.Now.AddDays(-1)
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.Contains(failedProperties, "MustBeGreaterThanProperty");
        }
    }
}
