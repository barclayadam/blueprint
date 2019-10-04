using System;
using System.Security.Cryptography;

namespace Blueprint.Core.Security
{
    /// <summary>
    /// Salted password hashing with PBKDF2-SHA1.
    /// Author: havoc AT defuse.ca
    /// www: http://crackstation.net/hashing-security.htm.
    /// </summary>
    [Serializable]
    public class PasswordHash
    {
        // The following constants may be changed without breaking existing hashes.
        public static readonly int SaltBytes = 24;

        public static readonly int HashBytes = 24;

        // Use the current year to continually increase the iteration rate in an attempt
        // to keep security up to date with increases in computing power without monitoring the
        // base value constantly.
        //
        // 2017 == 104913
        // 2018 == 105832
        // 2019 == 106859
        // 2020 == 108000
        // 2021 == 109261
        // 2022 == 110648
        public static readonly int Pbkdf2Iterations = 100_000 + (int)Math.Pow(DateTime.UtcNow.Year - 2000, 3);

        private readonly string hashedPassword;

        private PasswordHash(string hashedPassword)
        {
            Guard.NotNull(nameof(hashedPassword), hashedPassword);

            this.hashedPassword = hashedPassword;
        }

        /// <summary>
        /// Creates a salted PBKDF2 hash of the password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hash of the password.</returns>
        public static PasswordHash CreateHash(string password)
        {
            // Generate a random salt
            using (var csprng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[SaltBytes];
                csprng.GetBytes(salt);

                // Hash the password and encode the parameters
                var hash = Pbkdf2(password, salt, Pbkdf2Iterations, HashBytes);
                var hashedPassword = Pbkdf2Iterations + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);

                return new PasswordHash(hashedPassword);
            }
        }

        /// <summary>
        /// From a string representation of a <see cref="PasswordHash" /> creates the original
        /// password hash representation.
        /// </summary>
        /// <param name="hash">The string representation of the password hash.</param>
        /// <returns>The reconstructed password hash.</returns>
        public static PasswordHash FromHash(string hash)
        {
            return new PasswordHash(hash);
        }

        /// <summary>
        /// Validates that the specified password matches this hashed password.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns><c>true</c> if the password is correct. <c>false</c> otherwise.</returns>
        public bool IsValid(string password)
        {
            // Extract the parameters from the hash
            var split = hashedPassword.Split(':');
            var iterations = int.Parse(split[0]);
            var salt = Convert.FromBase64String(split[1]);
            var hash = Convert.FromBase64String(split[2]);

            var testHash = Pbkdf2(password, salt, iterations, hash.Length);

            return SlowEquals(hash, testHash);
        }

        public bool CanUpgrade()
        {
            // Extract the parameters from the hash
            var split = hashedPassword.Split(':');
            var iterations = int.Parse(split[0]);

            return iterations < Pbkdf2Iterations;
        }

        /// <summary>
        /// Returns the string representation of this password, which is how the hash should
        /// be stored in the persistent store of the application.
        /// </summary>
        /// <returns>The string representation of this hash.</returns>
        /// <seealso cref="FromHash" />
        public override string ToString()
        {
            return hashedPassword;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((PasswordHash)obj);
        }

        public override int GetHashCode()
        {
            return hashedPassword.GetHashCode();
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;

            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }

            return diff == 0;
        }

        /// <summary>
        /// Computes the PBKDF2-SHA1 hash of a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The PBKDF2 iteration count.</param>
        /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
        /// <returns>A hash of the password.</returns>
        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations })
            {
                return pbkdf2.GetBytes(outputBytes);
            }
        }

        private bool Equals(PasswordHash other)
        {
            return string.Equals(hashedPassword, other.hashedPassword);
        }
    }
}
