using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Blueprint.Http.Formatters;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// A <see cref="TextBodyParser"/> for JSON content that uses <see cref="System.Text.Json.JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftJsonBodyParser : TextBodyParser
    {
        private static readonly ApiExceptionFactory _invalidJson = new ApiExceptionFactory(
            "The JSON payload is invalid",
            "invalid_json",
            HttpStatusCode.BadRequest);

        private readonly JsonSerializer _bodyJsonSerializer;

        /// <summary>
        /// Initializes a new instance of <see cref="NewtonsoftJsonBodyParser"/>.
        /// </summary>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/>.</param>
        internal NewtonsoftJsonBodyParser(JsonSerializerSettings settings)
        {
            this._bodyJsonSerializer = JsonSerializer.Create(settings);

            this.SupportedEncodings.Add(UTF8EncodingWithoutBOM);

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
                // This is copied from JsonConvert.PopulateObject to avoid creating a new JsonSerializer on each
                // execution.
                using var stringReader = new StreamReader(inputStream);
                using var jsonReader = new JsonTextReader(stringReader) { CloseInput = false };

                try
                {
                    this._bodyJsonSerializer.Populate(jsonReader, context.Instance);
                }
                catch (JsonException e)
                {
                    throw _invalidJson.Create(e.Message);
                }

                if (await jsonReader.ReadAsync() && jsonReader.TokenType != JsonToken.Comment)
                {
                    throw _invalidJson.Create("Additional text found in JSON");
                }
            }
            finally
            {
                if (usesTranscodingStream)
                {
                    // TODO: When we target .net 5.0 can make use of built-in transcoding Stream with async Dispose
                    // await inputStream.DisposeAsync();
                }
            }

            // We have populated the object given to us, return as-is.
            return context.Instance;
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
}
