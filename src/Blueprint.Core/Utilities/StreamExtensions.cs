using System.IO;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Provides a number of extension methods to the built-in <see cref="Stream"/> class.
    /// </summary>
    public static class StreamExtensions
    {
        private const int SixteenKilobytes = 16 * 1024;

        /// <summary>
        /// Given an input stream will read fully to the end, starting at the current
        /// pointer position of the stream, returning a byte array containing the
        /// remaining contents of the given stream.
        /// </summary>
        /// <param name="input">
        /// The stream to read from.
        /// </param>
        /// <returns>
        /// A byte array containing the remaining contents of the given stream.
        /// </returns>
        public static byte[] ReadFully(this Stream input)
        {
            Guard.NotNull(nameof(input), input);

            var buffer = new byte[SixteenKilobytes];

            using (var ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}