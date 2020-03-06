// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/aspnet/Mvc/blob/8d66f104f7f2ca42ee8b21f75b0e2b3e1abe2e00/src/Microsoft.AspNetCore.Mvc.Core/Infrastructure/IHttpRequestStreamReaderFactory.cs

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Api.Http.Infrastructure
{
    /// <summary>
    /// Creates <see cref="TextReader"/> instances for reading from <see cref="HttpRequest.Body"/>.
    /// </summary>
    internal interface IHttpRequestStreamReaderFactory
    {
        /// <summary>
        /// Creates a new <see cref="TextReader"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>, usually <see cref="HttpRequest.Body"/>.</param>
        /// <param name="encoding">The <see cref="Encoding"/>, usually <see cref="Encoding.UTF8"/>.</param>
        /// <returns>A <see cref="TextReader"/>.</returns>
        TextReader CreateReader(Stream stream, Encoding encoding);
    }
}
