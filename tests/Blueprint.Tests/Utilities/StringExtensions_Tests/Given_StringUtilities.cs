using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.StringExtensions_Tests;

public class Given_StringUtilities
{
    [Test]
    [TestCase("imAString", "ImAString")]
    [TestCase("imAlsoString", "ImAlsoString")]
    [TestCase("ImAlsoString", "ImAlsoString")]
    [TestCase("im_a_string", "ImAString")]
    [TestCase("im a string", "ImAString")]
    [TestCase("ABCAcronym", "AbcAcronym")]
    [TestCase("im_a_ABCAcronym", "ImAAbcAcronym")]
    [TestCase("im a ABCAcronym", "ImAAbcAcronym")]
    [TestCase("8ball", "8Ball")]
    [TestCase("im a 8ball", "ImA8Ball")]
    [TestCase("IM_ALL_CAPS", "ImAllCaps")]
    [TestCase("IM ALSO ALL CAPS", "ImAlsoAllCaps")]
    [TestCase("i-have-dashes", "IHaveDashes")]
    [TestCase("a8word_another_word", "A8WordAnotherWord")]
    public void WhenGivenString_ShouldPascalCaseIt(string input, string expectedResult)
    {
        var result = input.ToPascalCase();

        result.Should().Be(expectedResult);
    }

    [Test]
    [TestCase("ImACaseSensitiveString", "imACaseSensitiveString", false)]
    [TestCase("", "", true)]
    [TestCase("ImTheSame", "ImTheSame", true)]
    [TestCase("ImTheSame\n", "ImTheSame", true)]
    [TestCase("ImTheSame\n", "ImTheSame\n", true)]
    [TestCase("ImTheSame\n", "\nImTheSame", true)]
    [TestCase("ImTheSame\n", "\nImThe\r\nSame", true)]
    [TestCase("Im\r\n\r\n\n\n\nTheSame\n", "\nImThe\r\nSame", true)]
    public void When_Comparing_Ignoring_New_Lines_Then_Success(string a, string b, bool expectedResult)
    {
        StringExtensions.EqualsIgnoringNewlines(a, b).Should().Be(expectedResult);
    }
}
