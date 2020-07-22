using System;
using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.GreaterThanPropertyAttribute_Tests
{
    public class Given_A_Non_Comparable_Value
    {
        public class NonComparable {}

        public class Validatable
        {
            public int? PropertyToCheckAgainst { get; set; }

            [GreaterThanProperty("PropertyToCheckAgainst")]
            public NonComparable MustBeGreaterThanProperty { get; set; }
        }

        [Test]
        public void When_Value_Type_Is_Not_Numeric_Then_Exception_Is_Thrown()
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = 1,
                MustBeGreaterThanProperty = new NonComparable()
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act / Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await validator.GetValidationResultsAsync(validatable, null));
        }
    }
}
