using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Blueprint.Core.Utilities
{
    public static class CompressionUtilities
    {
        public static string Compress(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string Decompress(string s)
        {
            var bytes = Convert.FromBase64String(s);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }

        public static byte[] CompressToBytes(string s)
        {
            var data = Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream();

            using (Stream ds = new GZipStream(stream, CompressionMode.Compress))
            {
                ds.Write(data, 0, data.Length);
            }

            return stream.ToArray();
        }

        public static string DecompressFromBytes(Stream compressedStream)
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return Encoding.UTF8.GetString(resultStream.ToArray());
            }
        }
    }
}