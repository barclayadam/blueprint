using System.Security.Cryptography;
using System.Text;

namespace Blueprint.Core.Security
{
    /// <summary>
    /// A Key / Initialisation Vector pair which is to be provided to an encryption service to
    /// generate the required values when performing encryption or decryption.
    /// </summary>
    public class KeyIVPair
    {
        private readonly string password;

        private readonly byte[] salt;

        private KeyIVPair(string password)
        {
            this.password = password;
            salt = GenerateSalt(password);
        }

        /// <summary>
        /// Creates a new KeyIVPair from the given password string.
        /// </summary>
        /// <param name="password">The unique password key to be used.</param>
        /// <returns>A KeyIVPair based on the given password.</returns>
        public static KeyIVPair FromPassword(string password)
        {
            Guard.NotNull(nameof(password), password);

            return new KeyIVPair(password);
        }

        /// <summary>
        /// Gets a byte array of the given size to be used as an initialisation vector in a symmetric
        /// cryptography algorithm..
        /// </summary>
        /// <param name="size">The size of the key to generate.</param>
        /// <returns>A byte array to be used as an initialisation vector.</returns>
        public byte[] GetIV(int size)
        {
            using (var pdb = new Rfc2898DeriveBytes(password, salt))
            {
                return pdb.GetBytes(size);
            }
        }

        /// <summary>
        /// Gets a byte array of the given size to be used as a key in a symmetric cryptography algorithm.
        /// </summary>
        /// <param name="size">The size of the key to generate.</param>
        /// <returns>A byte array to be used as a key.</returns>
        public byte[] GetKey(int size)
        {
            using (var pdb = new Rfc2898DeriveBytes(password, salt))
            {
                return pdb.GetBytes(size);
            }
        }

        private static byte[] GenerateSalt(string password)
        {
            return Encoding.Default.GetBytes(
                                             "A salt must always be the same for Rfc2898DeriveBytes to give you" +
                                             "the same password bytes and IV. Using password: " + password);
        }
    }
}
