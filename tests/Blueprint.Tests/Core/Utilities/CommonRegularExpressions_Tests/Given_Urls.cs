using Blueprint.Core.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Utilities.CommonRegularExpressions_Tests
{
    public class Given_Urls
    {
        [Test]
        [TestCase("")]
        [TestCase("hjndhdjhsjdndj")]
        [TestCase("http://")] // Only protocol
        public void When_Invalid_Url_Then_No_Match(string url)
        {
            Assert.IsFalse(CommonRegularExpressions.UrlOnly.IsMatch(url), "Matched invalid value: " + url);
        }

        [Test]
        [TestCase("http://www.google.co.uk")]
        [TestCase(" http://www.google.co.uk ")] // Allow whitespace either side
        [TestCase("hTTP://www.google.CO.uk")] // Ensure case-sensitivity has no affect
        [TestCase("www.google.co.uk")]
        [TestCase("google.co.uk")]
        [TestCase("www.google.com")]
        [TestCase("www.google.info")]
        [TestCase("www.google.limited")]
        [TestCase("www.google.plumbing")]
        [TestCase("www.google.aaaaaaaaaa")]
        [TestCase("www.google.co.uk/search")]
        [TestCase("www.google.co.uk/search/")]
        [TestCase("www.google.co.uk/search.aspx")]
        [TestCase("www.google.co.uk/search.aspx?q=Something")]
        [TestCase("www.google.co.uk/search.php?q=Something+More")]
        [TestCase("www.google.co.uk?q=Something+More")]
        [TestCase("www.google.co.uk/search/do.php?q=Something+More")]
        [TestCase("www.google.co.uk/search(advanced)")]
        public void When_Valid_Url_Then_Match(string url)
        {
            Assert.IsTrue(CommonRegularExpressions.UrlOnly.IsMatch(url), "Failed to match " + url);
        }

        [Test]
        public void When_TopLevel_Domain_63_Characters_Then_Match()
        {
            var tld = new string('a', 63);
            var domain = "www.example." + tld;

            Assert.IsTrue(CommonRegularExpressions.UrlOnly.IsMatch(domain), "Failed to match " + domain);
        }

        [Test]
        public void When_TopLevel_Domain_64_Characters_Then_No_Match()
        {
            var tld = new string('a', 64);
            var domain = "www.example." + tld;

            Assert.IsFalse(CommonRegularExpressions.UrlOnly.IsMatch(domain), "Failed to match " + domain);
        }
    }
}