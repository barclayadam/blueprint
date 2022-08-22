using Blueprint.Utilities;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.CommonRegularExpressions_Tests;

public class Given_Emails
{
    [Test]
    [TestCase("")]
    [TestCase("test@...")]
    [TestCase("test")]
    [TestCase("test@")]
    public void When_Invalid_Email_Then_No_Match(string email)
    {
        Assert.IsFalse(CommonRegularExpressions.EmailOnly.IsMatch(email), "Matched invalid value: " + email);
    }

    [Test]
    [TestCase("test@example.com")]
    [TestCase(" test@example.com ")] // Allow whitespace either side
    [TestCase("test@127.0.0.1")]
    [TestCase("test@subdomain.domain.com")]
    [TestCase("test++example@subdomain.domain.com")]
    public void When_Valid_Email_Then_Match(string email)
    {
        Assert.IsTrue(CommonRegularExpressions.EmailOnly.IsMatch(email), "Failed to match " + email);
    }
}