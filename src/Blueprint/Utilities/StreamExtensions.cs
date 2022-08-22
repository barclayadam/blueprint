using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint.Utilities;

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

    /// <summary>
    /// Reads the given <see cref="Stream" /> fully, converting to a string using a new
    /// <see cref="StreamReader"/>.
    /// </summary>
    /// <remarks>
    /// <strong>IF</strong> the given input stream supports seeking, this method will
    /// call <see cref="Stream.Seek" /> to set the position to 0.
    /// </remarks>
    /// <param name="input">The stream to read.</param>
    /// <returns>The stream read as a string.</returns>
    public static Task<string> ReadFullyAsStringAsync(this Stream input)
    {
        Guard.NotNull(nameof(input), input);

        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }

        var streamReader = new StreamReader(input);
        return streamReader.ReadToEndAsync();
    }

    /// <summary>
    /// Converts this string to a Stream (currently backed by a <see cref="MemoryStream" />), using UTF8 to get
    /// the bytes from the string.
    /// </summary>
    /// <param name="value">The string to convert to an in-memory <see cref="Stream"/>.</param>
    /// <returns>A new stream of the UTF8 bytes of the string.</returns>
    public static Stream AsUtf8Stream(this string value)
    {
        return AsStream(Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// Creates a new <see cref="MemoryStream" /> and writes all bytes of this array to it, seeking
    /// back to 0 ready for immediate reading.
    /// </summary>
    /// <param name="bytes">The bytes to populate a stream with.</param>
    /// <returns>A new stream with the given bytes as content.</returns>
    public static Stream AsStream(this byte[] bytes)
    {
        var stream = new MemoryStream();

        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}