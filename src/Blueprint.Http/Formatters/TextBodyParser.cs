using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint.Http.Formatters;

/// <summary>
/// Reads an object from a request body with a text format.
/// </summary>
public abstract class TextBodyParser : BodyParser
{
    private static readonly ApiExceptionFactory _unsupportedContextType = new (
        "The encoding specified is unsupported",
        "unsupported_encoding",
        415);

    /// <summary>
    /// Returns UTF8 Encoding without BOM and throws on invalid bytes.
    /// </summary>
    protected static readonly Encoding UTF8EncodingWithoutBOM
        = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Returns UTF16 Encoding which uses littleEndian byte order with BOM and throws on invalid bytes.
    /// </summary>
    protected static readonly Encoding UTF16EncodingLittleEndian
        = new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);

    /// <summary>
    /// Gets the mutable collection of character encodings supported by
    /// this <see cref="TextBodyParser"/>. The encodings are
    /// used when reading the data.
    /// </summary>
    public IList<Encoding> SupportedEncodings { get; } = new List<Encoding>();

    /// <inheritdoc />
    public override Task<object> ReadAsync(BodyParserContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var selectedEncoding = this.SelectCharacterEncoding(context);

        if (selectedEncoding == null)
        {
            throw _unsupportedContextType.Create($"{context.HttpContext.Request.ContentType} is unsupported");
        }

        return this.ReadRequestBodyAsync(context, selectedEncoding);
    }

    /// <summary>
    /// Reads an object from the request body.
    /// </summary>
    /// <param name="context">The <see cref="BodyParserContext"/>.</param>
    /// <param name="encoding">The <see cref="Encoding"/> used to read the request body.</param>
    /// <returns>A <see cref="Task"/> that on completion deserializes the request body.</returns>
    protected abstract Task<object> ReadRequestBodyAsync(
        BodyParserContext context,
        Encoding encoding);

    /// <summary>
    /// Returns an <see cref="Encoding"/> based on <paramref name="context"/>'s
    /// character set.
    /// </summary>
    /// <param name="context">The <see cref="BodyParserContext"/>.</param>
    /// <returns>
    /// An <see cref="Encoding"/> based on <paramref name="context"/>'s
    /// character set. <c>null</c> if no supported encoding was found.
    /// </returns>
    protected Encoding SelectCharacterEncoding(BodyParserContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (this.SupportedEncodings.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(this.SupportedEncodings)} must be populated by ${this.GetType().FullName}");
        }

        var requestContentType = context.HttpContext.Request.ContentType;
        var requestMediaType = string.IsNullOrEmpty(requestContentType) ? default : new MediaType(requestContentType);
        if (requestMediaType.Charset.HasValue)
        {
            // Create Encoding based on requestMediaType.Charset to support charset aliases and custom Encoding
            // providers. Charset -> Encoding -> encoding.WebName chain canonicalizes the charset name.
            var requestEncoding = requestMediaType.Encoding;

            if (requestEncoding != null)
            {
                for (var i = 0; i < this.SupportedEncodings.Count; i++)
                {
                    if (string.Equals(
                            requestEncoding.WebName,
                            this.SupportedEncodings[i].WebName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return this.SupportedEncodings[i];
                    }
                }
            }

            // The client specified an encoding in the content type header of the request
            // but we don't understand it. In this situation we don't try to pick any other encoding
            // from the list of supported encodings and read the body with that encoding.
            // Instead, we return null and that will translate later on into a 415 Unsupported Media Type
            // response.
            return null;
        }

        // We want to do our best effort to read the body of the request even in the
        // cases where the client doesn't send a content type header or sends a content
        // type header without encoding. For that reason we pick the first encoding of the
        // list of supported encodings and try to use that to read the body. This encoding
        // is UTF-8 by default in our formatters, which generally is a safe choice for the
        // encoding.
        return this.SupportedEncodings[0];
    }
}