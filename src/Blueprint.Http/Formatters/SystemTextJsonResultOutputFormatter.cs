using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blueprint.Http.Formatters;

/// <summary>
/// An <see cref="IOperationResultOutputFormatter" /> that will use System.Text.Json to
/// format responses as JSON.
/// </summary>
public class SystemTextJsonResultOutputFormatter : IOperationResultOutputFormatter
{
    private readonly JsonSerializerOptions _options;
    private readonly List<MediaType> _supportedMediaTypes;

    /// <summary>
    /// Initialises a new instance of the <see cref="SystemTextJsonResultOutputFormatter" /> class.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    public SystemTextJsonResultOutputFormatter(JsonSerializerOptions options)
    {
        this._options = options;

        this._supportedMediaTypes = new List<MediaType>
        {
            new MediaType(MediaTypeHeaderValues.ApplicationJson.ToString()),
            new MediaType(MediaTypeHeaderValues.TextJson.ToString()),
            new MediaType(MediaTypeHeaderValues.ApplicationAnyJsonSyntax.ToString()),
        };
    }

    /// <inheritdoc />
    public bool IsSupported(OutputFormatterCanWriteContext context)
    {
        if (!context.ContentType.HasValue)
        {
            // application/json
            context.ContentType = this._supportedMediaTypes.First();
            return true;
        }

        foreach (var s in this._supportedMediaTypes)
        {
            if (context.ContentType.Value.IsSubsetOf(s))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task WriteAsync(OutputFormatterCanWriteContext context)
    {
        // We know ContentType has been set as we would have set to a default above in IsSupported
        context.Response.ContentType = context.ContentType.Value.ToString();

        var responseStream = context.Response.Body;

        await JsonSerializer.SerializeAsync(responseStream, context.Result, context.Result.GetType(), this._options);
        await responseStream.FlushAsync();
    }
}