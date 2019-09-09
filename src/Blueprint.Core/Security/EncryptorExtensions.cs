using System;
using System.Text;

namespace Blueprint.Core.Security
{
    /// <summary>
    /// Provides extension methods to <see cref="IEncryptor"/> to allow decrypting /
    /// encrypting different types of objects, using the underlying byte encryption
    /// methods of the extended IEncryptor.
    /// </summary>
    public static class EncryptorExtensions
    {
        /// <summary>
        /// Decrypts a string value, with the encrypted string having been obtained
        /// by encrypting using the <see cref="Encrypt(IEncryptor,string)"/> method.
        /// </summary>
        /// <param name="encryptor">The encryptor that will perform the actual decryption.</param>
        /// <param name="value">The value to be decrypted.</param>
        /// <returns>The decrypted value.</returns>
        public static string Decrypt(this IEncryptor encryptor, string value)
        {
            Guard.NotNull(nameof(encryptor), encryptor);
            Guard.NotNull(nameof(value), value);

            var valueBytes = Convert.FromBase64String(value);
            var decryptedBytes = encryptor.Decrypt(valueBytes);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        /// Encrypts the given string value, turning it into a Base64 encoded string
        /// that can be stored and passed around before being decrypted using the
        /// <see cref="Decrypt(IEncryptor, string)"/> method.
        /// </summary>
        /// <param name="encryptor">The encryptor that will perform the actual encryption.</param>
        /// <param name="value">The value to be encrypted.</param>
        /// <returns>The encrypted, base64 encoded version of the string passed in.</returns>
        public static string Encrypt(this IEncryptor encryptor, string value)
        {
            Guard.NotNull(nameof(encryptor), encryptor);
            Guard.NotNull(nameof(value), value);

            var valueBytes = Encoding.UTF8.GetBytes(value);
            var encryptedBytes = encryptor.Encrypt(valueBytes);

            return Convert.ToBase64String(encryptedBytes);
        }
    }
}