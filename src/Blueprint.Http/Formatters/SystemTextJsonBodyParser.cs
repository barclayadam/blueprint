using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Formatters;

/// <summary>
/// A <see cref="TextBodyParser"/> for JSON content that uses <see cref="JsonSerializer"/>.
/// </summary>
public class SystemTextJsonBodyParser : TextBodyParser
{
    private static readonly ApiExceptionFactory _invalidJson = new ApiExceptionFactory(
        "The JSON payload is invalid",
        "invalid_json",
        HttpStatusCode.BadRequest);

    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="SystemTextJsonBodyParser"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/>.</param>
    public SystemTextJsonBodyParser(JsonSerializerOptions options)
    {
        this._serializerOptions = options;

        this.SupportedEncodings.Add(UTF8EncodingWithoutBOM);

        // this.SupportedEncodings.Add(UTF16EncodingLittleEndian);

        this.SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
        this.SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
        this.SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);
    }

    /// <inheritdoc />
    protected sealed override async Task<object> ReadRequestBodyAsync(
        BodyParserContext context,
        Encoding encoding)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (encoding == null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        var httpContext = context.HttpContext;
        var (inputStream, usesTranscodingStream) = this.GetInputStream(httpContext, encoding);

        try
        {
            return await JsonSerializer.DeserializeAsync(inputStream, context.BodyType, this._serializerOptions);
        }
        catch (JsonException jsonException)
        {
            throw _invalidJson.Create(jsonException.Message);
        }
        catch (Exception exception) when (exception is FormatException || exception is OverflowException)
        {
            // The code in System.Text.Json never throws these exceptions. However a custom converter could produce these errors for instance when
            // parsing a value. These error messages are considered safe to report to users

            throw _invalidJson.Create(exception.Message);
        }
        finally
        {
            if (usesTranscodingStream)
            {
                // TODO: When we target .net 5.0 can make use of built-in transcoding Stream with async Dispose
                // await inputStream.DisposeAsync();
            }
        }
    }

    private (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext, Encoding encoding)
    {
        if (encoding.CodePage == Encoding.UTF8.CodePage)
        {
            return (httpContext.Request.Body, false);
        }

        throw new Exception("Cannot handle non UTF8 encoding");

        // TODO: When we target .net 5.0 can make use of built-in transcoding Stream
        // var inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, leaveOpen: true);
        // return (inputStream, true);
    }
}