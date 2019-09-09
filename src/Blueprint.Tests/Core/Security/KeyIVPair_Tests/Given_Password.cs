using Blueprint.Core.Security;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Security.KeyIVPair_Tests
{
    public class Given_Password
    {
        [Test]
        public void When_Creating_Then_Key_IV_Pair_Returned()
        {
            // Arrange
            var password = "Passwwword";
            var keyIVPair = KeyIVPair.FromPassword(password);

            // Act
            var key = keyIVPair.GetKey(100);
            var iv = keyIVPair.GetIV(100);

            // Assert
            key.Should().NotBeNull();
            iv.Should().NotBeNull();
            key.Should().BeEquivalentTo(iv);
        }

        [Test]
        public void When_Getting_Key_Twice_Then_It_Will_Be_The_Same_Both_Times()
        {
            // Arrange
            var password = "Passwwword";
            var firstKeyIVPair = KeyIVPair.FromPassword(password);
            var secondKeyIVPair = KeyIVPair.FromPassword(password);

            // Act
            var keyOne = firstKeyIVPair.GetKey(100);

            var keyTwo = secondKeyIVPair.GetKey(100);

            // Assert
            keyOne.Should().BeEquivalentTo(keyTwo);
        }
    }
}