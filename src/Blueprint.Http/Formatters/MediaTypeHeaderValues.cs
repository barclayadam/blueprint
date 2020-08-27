// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Taken from: https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Formatters/MediaTypeHeaderValues.cs
using Microsoft.Net.Http.Headers;

namespace Blueprint.Http.Formatters
{
    internal static class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationJson
            = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextJson
            = MediaTypeHeaderValue.Parse("text/json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationAnyJsonSyntax
            = MediaTypeHeaderValue.Parse("application/*+json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationXml
            = MediaTypeHeaderValue.Parse("application/xml").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextXml
            = MediaTypeHeaderValue.Parse("text/xml").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationAnyXmlSyntax
            = MediaTypeHeaderValue.Parse("application/*+xml").CopyAsReadOnly();
    }
}
