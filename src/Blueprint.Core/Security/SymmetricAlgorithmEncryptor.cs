using System.IO;
using System.Security.Cryptography;

namespace Blueprint.Core.Security
{
    /// <summary>
    /// Provides an implementation of IEncryptionService which uses a <see cref="System.Security.Cryptography.SymmetricAlgorithm"/>
    /// to provide the cryptographic implementation.
    /// </summary>
    /// <typeparam name="T">The algorithm class which this implementation uses.</typeparam>
    public class SymmetricAlgorithmEncryptor<T> : IEncryptor
            where T : SymmetricAlgorithm, new()
    {
        private const int IVSize = 128 / 8;

        private const int KeySize = 256 / 8;

        private readonly CipherMode cipherMode;

        private readonly KeyIVPair keyIVPair;

        private readonly PaddingMode paddingMode;

        /// <summary>
        /// Initializes a new instance of the SymmetricAlgorithmEncryptor class.
        /// </summary>
        /// <param name="cipherMode">The cipher mode to use.</param>
        /// <param name="paddingMode">The padding mode to use.</param>
        /// <param name="keyIVPair">The Key/IV pair to use.</param>
        public SymmetricAlgorithmEncryptor(CipherMode cipherMode, PaddingMode paddingMode, KeyIVPair keyIVPair)
        {
            Guard.EnumDefined("cipherMode", cipherMode);
            Guard.EnumDefined("paddingMode", paddingMode);
            Guard.NotNull(nameof(keyIVPair), keyIVPair);

            this.cipherMode = cipherMode;
            this.paddingMode = paddingMode;
            this.keyIVPair = keyIVPair;
        }

        /// <summary>
        /// Decrypts the given byte array.
        /// </summary>
        /// <param name="value">Byte array value to be decrypted.</param>
        /// <returns>Decrypted byte array.</returns>
        public byte[] Decrypt(byte[] value)
        {
            using (var provider = CreateProvider())
            {
                return Transform(value, provider.CreateDecryptor());
            }
        }

        /// <summary>
        /// Encrypts the given byte array.
        /// </summary>
        /// <param name="value">Byte array value to be encrypted.</param>
        /// <returns>Encrypted byte array.</returns>
        public byte[] Encrypt(byte[] value)
        {
            using (var provider = CreateProvider())
            {
                return Transform(value, provider.CreateEncryptor());
            }
        }

        private static byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
        {
            using (var memStream = new MemoryStream())
            using (var cryptStream = new CryptoStream(memStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptStream.Write(input, 0, input.Length);
                cryptStream.FlushFinalBlock();

                memStream.Position = 0;
                var result = memStream.ToArray();

                return result;
            }
        }

        private T CreateProvider()
        {
            return new T
            {
                    Key = keyIVPair.GetKey(KeySize),
                    IV = keyIVPair.GetIV(IVSize),
                    Padding = paddingMode,
                    Mode = cipherMode,
            };
        }
    }
}
