using System;
using System.IO;
using Blueprint.Core;
using NLog;

namespace Blueprint.Api
{
    public class Base64FileData
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Base64FileData(byte[] data)
        {
            Data = new MemoryStream(data);
        }

        public Base64FileData(Stream data)
        {
            Data = data;
        }

        public Stream Data { get; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public static Base64FileData Decode(string input)
        {
            Guard.NotNullOrEmpty(nameof(input), input);

            Log.Info("Decoding posted file input.");

            // filename:test-cv3.doc;data:application/msword;base64,0M8...
            // filename:<value>;data:<value>;base64,<data>
            var parts = input.Split(';');

            if (parts.Length != 3)
            {
                throw new ArgumentException($"Unable to decode file, incorrect number of parameters! expected 3 but got {parts.Length}");
            }

            var filename = parts[0].Replace("filename:", string.Empty);
            var contentType = parts[1].Replace("data:", string.Empty);
            var data = parts[2].Substring(parts[2].IndexOf(',') + 1);

            var bytes = Convert.FromBase64String(data);

            Log.Info($"File decoded succesfully. filename={filename}; length={bytes.Length}");

            return new Base64FileData(bytes)
            {
                FileName = filename,
                ContentType = contentType,
            };
        }
    }
}
