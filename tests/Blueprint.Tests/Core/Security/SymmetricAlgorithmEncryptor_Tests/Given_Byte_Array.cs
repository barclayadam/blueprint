using System.Security.Cryptography;
using Blueprint.Core.Security;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Security.SymmetricAlgorithmEncryptor_Tests
{
    public class Given_Byte_Array
    {
        [Test]
        public void MustReturnEncrypted_WhenEncryptingByteArray()
        {
            // Arrange
            var encryptor = new SymmetricAlgorithmEncryptor<AesCryptoServiceProvider>(
                    CipherMode.CFB, PaddingMode.PKCS7, KeyIVPair.FromPassword("bgSGDjsbnds"));
            var original = new byte[] { 0, 3, 4, 6, 7, 3, 2, 4, 65, 6, 3, 2, 2, 5, 6, 7, 8, 4, 2, 6 };

            // Act
            var encrypted = encryptor.Encrypt(original);

            // Assert
            encrypted.Should().NotBeEquivalentTo(original);
        }

        [Test]
        public void MustRoundTrip_WhenEncryptingThenDecryptingByteArray()
        {
            // Arrange
            var encryptor = new SymmetricAlgorithmEncryptor<AesCryptoServiceProvider>(
                    CipherMode.CFB, PaddingMode.PKCS7, KeyIVPair.FromPassword("bgSGDjsbnds"));
            var original = new byte[] { 0, 3, 4, 6, 7, 3, 2, 4, 65, 6, 3, 2, 2, 5, 6, 7, 8, 4, 2, 6 };
            var encrypted = encryptor.Encrypt(original);

            // Act
            var decrypted = encryptor.Decrypt(encrypted);

            // Assert
            decrypted.Should().BeEquivalentTo(original);
        }
    }
}