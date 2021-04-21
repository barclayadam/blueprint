using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Http.Middleware
{
    /// <summary>
    /// A middleware component that will take any <see cref="ResourceEvent" /> objects returned from
    /// API operations and populate the return with extra information such as the resource itself
    /// as it's canonical 'self' link for consumption elsewhere.
    /// </summary>
    public static class ResourceEventHandler
    {
        public static async Task HandleAsync(
            IResourceEventRepository resourceEventRepository,
            IApiLinkGenerator apiLinkGenerator,
            ApiOperationContext context,
            OperationResult result)
        {
            if (result is OkResult okResult)
            {
                var innerResult = okResult.Content;

                if (innerResult is ResourceEvent resourceEvent)
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<ResourceEventHandlerMiddlewareBuilder>>();
                    var metadataProviders = context.ServiceProvider.GetServices<IContextMetadataProvider>();

                    void AddMetadata(string k, object v) => resourceEvent.WithMetadata(k, v);

                    context.UserAuthorisationContext?.PopulateMetadata(AddMetadata);

                    resourceEvent.CorrelationId = Activity.Current?.Id;

                    foreach (var p in metadataProviders)
                    {
                        await p.PopulateMetadataAsync(context, AddMetadata);
                    }

                    var selfLink = context.DataModel.GetLinkFor(resourceEvent.ResourceType, "self");

                    if (selfLink == null)
                    {
                        logger.LogWarning(
                            "No self link exists. Link and changes will not be populated. resource_type={0}",
                            resourceEvent.ResourceType.Name);
                    }
                    else
                    {
                        resourceEvent.Href = apiLinkGenerator.CreateUrl(selfLink, resourceEvent.Data);

                        await PopulateChangesAsync(resourceEventRepository, resourceEvent);
                    }

                    await resourceEventRepository.AddAsync(resourceEvent);
                }
            }
        }

        private static async Task PopulateChangesAsync(
            IResourceEventRepository resourceEventRepository,
            ResourceEvent resourceEvent)
        {
            if (resourceEvent.ChangeType == ResourceEventChangeType.Updated)
            {
                var previousResource =
                    await resourceEventRepository.GetCurrentDataAsync(resourceEvent.Href, resourceEvent.ResourceType);

                // There are cases where the old resource will not exist because resources being saved was a new
                // introduction after being in prod for over a year
                if (previousResource != null)
                {
                    resourceEvent.ChangedValues = ObjectComparer.GetPreviousValues(
                        previousResource,
                        resourceEvent.Data);
                }
            }
        }
    }
}
