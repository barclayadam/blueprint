using System;
using System.Linq;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.RandomStringGenerator_Tests;

public class Given_RandomStringGenerator
{
    [Test]
    public void When_Generating_A_Zero_Length_String_It_Should_Throw_An_InvalidOperationException()
    {
        // Arrange
        var stringGenerator = new RandomStringGenerator();

        stringGenerator.WithCharacter('a');

        // Act
        var exception = Assert.Catch<InvalidOperationException>(() => stringGenerator.Generate());

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("No length has been defined. See OfLength method.");
    }

    public class When_Generating_A_Non_Zero_Length_String
    {
        [Test]
        public void With_A_Single_Character_It_Should_Generate_A_String_Of_The_Desired_Length_Of_Only_That_Character()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator.WithCharacter('a').OfLength(3);

            // Act
            var generated = stringGenerator.Generate();

            // Assert
            generated.Should().Be("aaa");
        }

        [Test]
        public void With_A_Single_Explicit_Character_Set_It_Should_Generate_A_String_Of_The_Desired_Length_With_Characters_From_That_Set()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator
                .WithCharacterSet(Enumerable.Range('a', ('z' - 'a') + 1).Select(i => (char)i))
                .OfLength(3);

            // Act
            var generated = stringGenerator.Generate();

            // Assert
            generated.Should().MatchRegex("^[a-z]{3}$");
        }

        [Test]
        public void With_A_Single_Implicit_Character_Set_It_Should_Generate_A_String_Of_The_Desired_Length_With_Characters_From_That_Set()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator
                .WithCharacterSet('a', 'z')
                .OfLength(3);

            // Act
            var generated = stringGenerator.Generate();

            // Assert
            generated.Should().MatchRegex("^[a-z]{3}$");
        }

        [Test]
        public void With_Length_Set_Twice_It_Should_Use_The_Last_Set_Length()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator
                .WithCharacter('a')
                .OfLength(17)
                .OfLength(3);

            // Act
            var generated = stringGenerator.Generate();

            // Assert
            generated.Should().Be("aaa");
        }

        [Test]
        public void With_Multiple_Character_Sets_It_Should_Generate_A_String_Of_The_Desired_Length_With_Characters_From_Each_Set()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator
                .WithCharacterSet('a', 'z')
                .WithCharacterSet('A', 'Z')
                .WithCharacterSet('0', '9')
                .OfLength(3);

            // Act
            var generated = stringGenerator.Generate();

            // Assert
            generated.Should().MatchRegex("^[a-z][A-Z][0-9]$");
        }

        [Test]
        public void With_No_Character_Definitions_It_Should_Throw_An_InvalidOperationException()
        {
            // Arrange
            var stringGenerator = new RandomStringGenerator();

            stringGenerator.OfLength(1);

            // Act
            var exception = Assert.Catch<InvalidOperationException>(() => stringGenerator.Generate());

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be("No character sets have been defined. See the WithCharacterSet method.");
        }
    }
}