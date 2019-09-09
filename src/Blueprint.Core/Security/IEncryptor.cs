namespace Blueprint.Core.Security
{
    /// <summary>
    /// Provides encryption and decryption services, working at the byte array level.
    /// </summary>
    public interface IEncryptor
    {
        /// <summary>
        /// Decrypts the given byte array.
        /// </summary>
        /// <param name="value">Byte array value to be decrypted.</param>
        /// <returns>Decrypted byte array.</returns>
        byte[] Decrypt(byte[] value);

        /// <summary>
        /// Encrypts the given byte array.
        /// </summary>
        /// <param name="value">Byte array value to be encrypted.</param>
        /// <returns>Encrypted byte array.</returns>
        byte[] Encrypt(byte[] value);
    }
}