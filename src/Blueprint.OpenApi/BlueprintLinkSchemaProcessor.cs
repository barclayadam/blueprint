using System.Collections.Generic;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Http;
using NJsonSchema.Generation;

namespace Blueprint.OpenApi
{
    /// <summary>
    /// A <see cref="ISchemaProcessor" /> that will add a vendor extension of <c>x-links</c> to
    /// a generated schema to represents <see cref="Link" />s that are registered for that
    /// API resource type.
    /// </summary>
    public class BlueprintLinkSchemaProcessor : ISchemaProcessor
    {
        private readonly ApiDataModel apiDataModel;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintLinkSchemaProcessor" /> class.
        /// </summary>
        /// <param name="apiDataModel">The <see cref="ApiDataModel" /> being processed.</param>
        public BlueprintLinkSchemaProcessor(ApiDataModel apiDataModel)
        {
            this.apiDataModel = apiDataModel;
        }

        /// <inheritdoc />
        public void Process(SchemaProcessorContext context)
        {
            if (!typeof(ApiResource).IsAssignableFrom(context.Type))
            {
                return;
            }

            var resourceLinks = apiDataModel
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
}
