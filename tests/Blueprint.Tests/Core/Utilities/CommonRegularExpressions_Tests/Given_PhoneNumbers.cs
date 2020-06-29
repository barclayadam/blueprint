using Blueprint.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.CommonRegularExpressions_Tests
{
    public class Given_PhoneNumbers
    {
        [Test]
        [TestCase("")]
        [TestCase("0123456sd")] // Contains alpha characters
        [TestCase("1234567890123456789012345678901")] // Greater than 30 characters
        public void When_Invalid_PhoneNumber_Then_No_Match(string email)
        {
            Assert.IsFalse(CommonRegularExpressions.PhoneNumberOnly.IsMatch(email), "Matched invalid value: " + email);
        }

        [Test]
        [TestCase("01329854512")]
        [TestCase(" 01329854512 ")] // Allow whitespace either side
        [TestCase("01329 854512")]
        [TestCase("01329-854512")]
        [TestCase("(01329)854512")]
        [TestCase("(01329)854512")]
        [TestCase("123456789012345678901234567890")] // 30 characters
        public void When_Valid_PhoneNumber_Then_Match(string email)
        {
            Assert.IsTrue(CommonRegularExpressions.PhoneNumberOnly.IsMatch(email), "Failed to match " + email);
        }
    }
}
