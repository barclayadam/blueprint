using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Utilities
{
    /// <summary>
    /// A random string generator that can, given sets of characters to include in a generated string and
    /// a desired length, generate random strings.
    /// </summary>
    public class RandomStringGenerator
    {
        private readonly List<char[]> _characterSets = new List<char[]>();

        private readonly Random _random = new Random();

        private int _length;

        /// <summary>
        /// Generates a random string containing characters from any defined characters sets or
        /// the desired length.
        /// </summary>
        /// <returns>A random string.</returns>
        /// <exception cref="InvalidOperationException">If no character sets or length has been defined.</exception>
        public string Generate()
        {
            if (!this._characterSets.Any())
            {
                throw new InvalidOperationException("No character sets have been defined. See the WithCharacterSet method.");
            }

            if (this._length <= 0)
            {
                throw new InvalidOperationException("No length has been defined. See OfLength method.");
            }

            return string.Join(string.Empty, Enumerable.Range(0, this._length).Select(this.GetFromCharacterSet).ToArray());
        }

        /// <summary>
        /// Sets the desired length of the random string when next calling <see cref="Generate"/>.
        /// </summary>
        /// <param name="desiredLength">The desired length of the string to be generated.</param>
        /// <returns>This RandomStringGenerator for further configuration.</returns>
        public RandomStringGenerator OfLength(int desiredLength)
        {
            Guard.GreaterThan(nameof(desiredLength), desiredLength, 0);

            this._length = desiredLength;

            return this;
        }

        /// <summary>
        /// Adds the specified character as a 'set', indicating that this single character could
        /// be included in the output.
        /// </summary>
        /// <param name="character">The character to be included in the output.</param>
        /// <returns>This RandomStringGenerator for further configuration.</returns>
        public RandomStringGenerator WithCharacter(char character)
        {
            this._characterSets.Add(new[] { character });

            return this;
        }

        /// <summary>
        /// Adds the specified character set, indicating that the output can contain any one of the
        /// characters on this set.
        /// </summary>
        /// <param name="characterSet">The character set to be included in the output.</param>
        /// <returns>This RandomStringGenerator for further configuration.</returns>
        public RandomStringGenerator WithCharacterSet(IEnumerable<char> characterSet)
        {
            Guard.NotNull(nameof(characterSet), characterSet);

            this._characterSets.Add(characterSet.ToArray());

            return this;
        }

        /// <summary>
        /// Adds a character set that includes all characters from 'start' to 'end', inclusive, using
        /// the ASCII codes as the definition of a range (e.g 'a' to 'z' to include the lowercase alphabet).
        /// </summary>
        /// <param name="start">The character from which to start (inclusive).</param>
        /// <param name="end">The character to end (inclusive).</param>
        /// <returns>This RandomStringGenerator for further configuration.</returns>
        public RandomStringGenerator WithCharacterSet(char start, char end)
        {
            this._characterSets.Add(Enumerable.Range(start, (end - start) + 1).Select(i => (char)i).ToArray());

            return this;
        }

        private char GetFromCharacterSet(int i)
        {
            var characterSet = this._characterSets.ElementAt(i % this._characterSets.Count());
            var randomIndex = this._random.Next(0, characterSet.Length);

            return characterSet[randomIndex];
        }
    }
}
