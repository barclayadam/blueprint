using Blueprint.Core.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.CommonRegularExpressions_Tests
{
    public class Given_Postcodes
    {
        [Test]
        [TestCase("")]
        [TestCase("QWERTY")]
        [TestCase("PO12")]
        [TestCase("P0124ER")]
        public void When_Invalid_Postcode_Then_No_Match(string postcode)
        {
            Assert.IsFalse(
                           CommonRegularExpressions.UKPostcodeOnly.IsMatch(postcode),
                           "Matched invalid value: " + postcode);
        }

        [Test]
        [TestCase("PO124ER")]
        [TestCase(" PO124ER ")] // Allow whitespace either side
        [TestCase("PO12 4ER")]
        [TestCase("GIR 0AA")]
        [TestCase("GIR0AA")]
        [TestCase("PO3 6JZ")]
        [TestCase("PO36JZ")]
        public void When_Valid_Postcode_Then_Match(string postcode)
        {
            Assert.IsTrue(CommonRegularExpressions.UKPostcodeOnly.IsMatch(postcode), "Failed to match " + postcode);
        }
    }
}