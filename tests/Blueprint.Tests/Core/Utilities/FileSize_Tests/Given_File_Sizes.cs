using System.Reflection;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.FileSize_Tests
{
    public class Given_File_Sizes
    {
        [TestCase(1, "1B")]
        [TestCase(2, "2B")]
        [TestCase(1024, "1KB")]
        [TestCase(2048, "2KB")]
        [TestCase(1048576, "1.00MB")]
        [TestCase(2097152, "2.00MB")]
        [TestCase(1073741824, "1.00GB")]
        [TestCase(2147483648, "2.00GB")]
        [TestCase(1099511627776, "1.00TB")]
        [TestCase(2199023255552, "2.00TB")]
        [Test]
        public void When_Formatting_As_FileSystem_Then_Output_Is_Dependent_On_Size(double bytes, string expectedFormat)
        {
            // Arrange
            var fileSize = FileSize.FromBytes(bytes);

            // Act
            var formatted = $"{fileSize:FS}";

            // Assert
            formatted.Should().Be(expectedFormat);
        }

        [TestCase(1, "1B")]
        [TestCase(2, "2B")]
        [TestCase(1024, "1KB")]
        [TestCase(2048, "2KB")]
        [TestCase(1048576, "1MB")]
        [TestCase(2097152, "2MB")]
        [TestCase(1073741824, "1GB")]
        [TestCase(2147483648, "2GB")]
        [TestCase(1099511627776, "1TB")]
        [TestCase(2199023255552, "2TB")]
        [Test]
        public void When_Formatting_As_FileSystem_With_Precision_Then_Output_Is_Dependent_On_Size(double bytes, string expectedFormat)
        {
            // Arrange
            var fileSize = FileSize.FromBytes(bytes);

            // Act
            var formatted = $"{fileSize:N0:FS}";

            // Assert
            formatted.Should().Be(expectedFormat);
        }

        [TestCase("Bytes", 100, "Bytes", 100)]
        [TestCase("Bytes", 1024, "Kilobytes", 1)]
        [TestCase("Bytes", 1048576, "Megabytes", 1)]
        [TestCase("Bytes", 1073741824, "Gigabytes", 1)]
        [TestCase("Bytes", 1099511627776, "Terabytes", 1)]
        [TestCase("Kilobytes", 1, "Bytes", 1024)]
        [TestCase("Megabytes", 1, "Bytes", 1048576)]
        [TestCase("Gigabytes", 1, "Bytes", 1073741824)]
        [TestCase("Terabytes", 1, "Bytes", 1099511627776)]
        [TestCase("Kilobytes", 1, "Kilobytes", 1)]
        [TestCase("Kilobytes", 512, "Megabytes", 0.5)]
        [TestCase("Kilobytes", 1024, "Megabytes", 1)]
        [TestCase("Megabytes", 1, "Megabytes", 1)]
        [TestCase("Megabytes", 512, "Gigabytes", 0.5)]
        [TestCase("Megabytes", 1024, "Gigabytes", 1)]
        [TestCase("Gigabytes", 1, "Gigabytes", 1)]
        [TestCase("Gigabytes", 512, "Terabytes", 0.5)]
        [TestCase("Gigabytes", 1024, "Terabytes", 1)]
        public void When_Converting_Between_Measurements(string input, double inputValue, string output, double outputValue)
        {
            // Arrange
            var createMethod = typeof(FileSize).GetMethod("From" + input, BindingFlags.Public | BindingFlags.Static);

            createMethod.Should().NotBeNull("Expected a static method: From" + input + "(long input) to exist on FileSize class");
            createMethod.GetParameters().Should().HaveCount(1, "Expected a static method: From" + input + "(long input) to exist on FileSize class");
            createMethod.GetParameters()[0].ParameterType.Should().Be(typeof(double), "Expected a static method: From" + input + "(long input) to exist on FileSize class");

            // Act
            var fileSize = (FileSize)createMethod.Invoke(null, new object[] { inputValue });

            // Assert
            var outputProperty = typeof(FileSize).GetProperty(output);

            outputProperty.Should().NotBeNull("Expected property '" + output + "' to exist.");
            outputProperty.GetValue(fileSize, null).Should().Be(outputValue, "Value of property '" + output + "' was incorrect.");
        }
    }
}
