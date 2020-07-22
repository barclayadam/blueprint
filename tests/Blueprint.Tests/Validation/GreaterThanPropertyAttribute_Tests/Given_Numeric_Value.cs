using System;
using System.Threading.Tasks;
using Blueprint.Validation;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.GreaterThanPropertyAttribute_Tests
{
    public class Given_Numeric_Value
    {
        public class Validatable
        {
            public object PropertyToCheckAgainst { get; set; }

            [GreaterThanProperty("PropertyToCheckAgainst")]
            public object MustBeGreaterThanProperty { get; set; }
        }

        [Test]
        public async Task When_Value_Is_Equal_To_PropertyToCheck_Value_Then_InValid()
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = 1,
                MustBeGreaterThanProperty = 1
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
                PropertyToCheckAgainst = 1,
                MustBeGreaterThanProperty = 2
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
                PropertyToCheckAgainst = 1,
                MustBeGreaterThanProperty = 0
            };

            var validator = new BlueprintValidator(new IValidationSource[] { new DataAnnotationsValidationSource() });

            // Act
            var failures = await validator.GetValidationResultsAsync(validatable, null);
            var failedProperties = failures.AsDictionary().Keys;

            // Assert
            CollectionAssert.Contains(failedProperties, "MustBeGreaterThanProperty");
        }

        [Test]
        [TestCase(typeof(byte), 9)]
        [TestCase(typeof(sbyte), 9)]
        [TestCase(typeof(short), 9)]
        [TestCase(typeof(ushort), 9)]
        [TestCase(typeof(int), 9)]
        [TestCase(typeof(uint), 9)]
        [TestCase(typeof(long), 9)]
        [TestCase(typeof(ulong), 9)]
        [TestCase(typeof(float), 9.1)]
        [TestCase(typeof(double), 9.1)]
        [TestCase(typeof(decimal), 9.1)]
        public async Task When_Value_Type_Is_Numeric_And_Greater_Than_Required_Then_Valid(Type type, object value)
        {
            // Arrange
            var validatable = new Validatable
            {
                PropertyToCheckAgainst = Convert.ChangeType(value, type),
                MustBeGreaterThanProperty = Convert.ChangeType(10, type)
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
