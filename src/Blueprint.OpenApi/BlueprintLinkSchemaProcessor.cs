using System.Collections.Generic;
using System.Linq;
using Blueprint.Http;
using NJsonSchema.Generation;

namespace Blueprint.OpenApi;

/// <summary>
/// A <see cref="ISchemaProcessor" /> that will add a vendor extension of <c>x-links</c> to
/// a generated schema to represents <see cref="Link" />s that are registered for that
/// API resource type.
/// </summary>
public class BlueprintLinkSchemaProcessor : ISchemaProcessor
{
    private readonly HttpHost _httpHost;

    /// <summary>
    /// Initialises a new instance of the <see cref="BlueprintLinkSchemaProcessor" /> class.
    /// </summary>
    /// <param name="httpHost">The <see cref="HttpHost" />.</param>
    public BlueprintLinkSchemaProcessor(HttpHost httpHost)
    {
        this._httpHost = httpHost;
    }

    /// <inheritdoc />
    public void Process(SchemaProcessorContext context)
    {
        if (!typeof(ApiResource).IsAssignableFrom(context.Type))
        {
            return;
        }

        var resourceLinks = this._httpHost
            .GetLinksForResource(context.Type)
            .Where(l => l.Rel != "self")
            .ToList();

        if (resourceLinks.Any())
        {
            context.Schema.ExtensionData ??= new Dictionary<string, object>();

            context.Schema.ExtensionData["x-links"] = resourceLinks.Select(l =>
            {
                var descriptor = l.OperationDescriptor;

                return new
                {
                    rel = l.Rel,
                    operationId = descriptor.Name,
                    method = descriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod,
                };
            });
        }
    }
}
