using System.Threading.Tasks;
using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.GreaterThanPropertyAttribute_Tests
{
    public class Given_A_Null_Object
    {
        public class Validatable
        {
            public int? PropertyToCheckAgainst { get; set; }

            [GreaterThanProperty("PropertyToCheckAgainst")]
            public int? MustBeGreaterThanProperty { get; set; }
        }

        [Test]
        public async Task When_Object_Is_Null_Then_Valid()
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = null,
                MustBeGreaterThanProperty = 1
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.DoesNotContain(failedProperties, "MustBeGreaterThanProperty");
        }
    }
}
