// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/aspnet/Mvc/blob/8d66f104f7f2ca42ee8b21f75b0e2b3e1abe2e00/src/Microsoft.AspNetCore.Mvc.Core/Infrastructure/IHttpResponseStreamWriterFactory.cs

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Infrastructure
{
    /// <summary>
    /// Creates <see cref="TextWriter"/> instances for writing to <see cref="HttpResponse.Body"/>.
    /// </summary>
    internal interface IHttpResponseStreamWriterFactory
    {
        /// <summary>
        /// Creates a new <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>, usually <see cref="HttpResponse.Body"/>.</param>
        /// <param name="encoding">The <see cref="Encoding"/>, usually <see cref="Encoding.UTF8"/>.</param>
        /// <returns>A <see cref="TextWriter"/>.</returns>
        TextWriter CreateWriter(Stream stream, Encoding encoding);
    }
}
