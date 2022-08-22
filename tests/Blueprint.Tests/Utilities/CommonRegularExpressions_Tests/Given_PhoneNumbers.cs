using Blueprint.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.CommonRegularExpressions_Tests;

public class Given_PhoneNumbers
{
    [Test]
    [TestCase("")]
    [TestCase("0123456sd")] // Contains alpha characters
    [TestCase("1234567890123456789012345678901")] // Greater than 30 characters
    public void When_Invalid_PhoneNumber_Then_No_Match(string phone)
    {
        Assert.IsFalse(CommonRegularExpressions.PhoneNumberOnly.IsMatch(phone), "Matched invalid value: " + phone);
    }

    [Test]
    [TestCase("01329854512")]
    [TestCase(" 01329854512 ")] // Allow whitespace either side
    [TestCase("01329 854512")]
    [TestCase("01329-854512")]
    [TestCase("(01329)854512")]
    [TestCase("(01329)854512")]
    [TestCase("123456789012345678901234567890")] // 30 characters
    [TestCase("392-248-7675 x027")] // With spaces and extension
    public void When_Valid_PhoneNumber_Then_Match(string phone)
    {
        Assert.IsTrue(CommonRegularExpressions.PhoneNumberOnly.IsMatch(phone), "Failed to match " + phone);
    }
}