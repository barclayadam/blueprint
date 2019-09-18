using System.Security.Cryptography;
using Blueprint.Core.Security;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Security.SymmetricAlgorithmEncryptor_Tests
{
    public class Given_String
    {
        [Test]
        public void MustReturnEncrypted_WhenEncryptingByteArray()
        {
            // Arrange
            var encryptor = new SymmetricAlgorithmEncryptor<AesCryptoServiceProvider>(
                    CipherMode.CFB, PaddingMode.PKCS7, KeyIVPair.FromPassword("bgSGDjsbnds"));
            var original = "The Quick Brown Fox Jumped Over The Lazy Dog";

            // Act
            var encrypted = encryptor.Encrypt(original);

            // Assert
            encrypted.Should().NotBe(original);
        }

        [Test]
        public void MustRoundTrip_WhenEncryptingThenDecryptingByteArray()
        {
            // Arrange
            var encryptor = new SymmetricAlgorithmEncryptor<AesCryptoServiceProvider>(
                    CipherMode.CFB, PaddingMode.PKCS7, KeyIVPair.FromPassword("bgSGDjsbnds"));
            var original = "The Quick Brown Fox Jumped Over The Lazy Dog";
            var encrypted = encryptor.Encrypt(original);

            // Act
            var decrypted = encryptor.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(original);
        }
    }
}