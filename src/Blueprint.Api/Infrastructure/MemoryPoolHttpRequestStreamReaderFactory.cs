// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/aspnet/Mvc/blob/8d66f104f7f2ca42ee8b21f75b0e2b3e1abe2e00/src/Microsoft.AspNetCore.Mvc.Core/Infrastructure/MemoryPoolHttpRequestStreamReaderFactory.cs

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Blueprint.Core;
using Microsoft.AspNetCore.WebUtilities;

namespace Blueprint.Api.Infrastructure
{
    /// <summary>
    /// An <see cref="IHttpRequestStreamReaderFactory"/> that uses pooled buffers.
    /// </summary>
    internal class MemoryPoolHttpRequestStreamReaderFactory : IHttpRequestStreamReaderFactory
    {
        /// <summary>
        /// The default size of created char buffers.
        /// </summary>
        private const int DefaultBufferSize = 1024; // 1KB - results in a 4KB byte array for UTF8.

        private readonly ArrayPool<byte> bytePool;
        private readonly ArrayPool<char> charPool;

        /// <summary>
        /// Creates a new <see cref="MemoryPoolHttpRequestStreamReaderFactory"/>.
        /// </summary>
        /// <param name="bytePool">
        /// The <see cref="ArrayPool{Byte}"/> for creating <see cref="T:byte[]"/> buffers.
        /// </param>
        /// <param name="charPool">
        /// The <see cref="ArrayPool{Char}"/> for creating <see cref="T:char[]"/> buffers.
        /// </param>
        public MemoryPoolHttpRequestStreamReaderFactory(
            ArrayPool<byte> bytePool,
            ArrayPool<char> charPool)
        {
            Guard.NotNull(nameof(bytePool), bytePool);
            Guard.NotNull(nameof(charPool), charPool);

            this.bytePool = bytePool;
            this.charPool = charPool;
        }

        /// <inheritdoc />
        public TextReader CreateReader(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new HttpRequestStreamReader(stream, encoding, DefaultBufferSize, bytePool, charPool);
        }
    }
}
