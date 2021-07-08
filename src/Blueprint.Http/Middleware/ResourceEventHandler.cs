using System;
using System.Threading.Tasks;
using Blueprint.Middleware;
using Blueprint.Validation;
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

                    logger.LogDebug("ResourceEvent found. Loading resource {ResourceType}", resourceEvent.ResourceType);

                    void AddMetadata(string k, object v) => resourceEvent.Metadata[k] = v;

                    context.UserAuthorisationContext?.PopulateMetadata(AddMetadata);

                    resourceEvent.CorrelationId = context.ApmSpan.TraceId;
                    resourceEvent.Operation = context.Operation;

                    var metadataProviders = context.ServiceProvider.GetServices<IContextMetadataProvider>();

                    foreach (var p in metadataProviders)
                    {
                        await p.PopulateMetadataAsync(context, AddMetadata);
                    }

                    var selfLink = context.DataModel.GetLinkFor(resourceEvent.ResourceType, "self");

                    if (selfLink == null)
                    {
                        logger.LogWarning(
                            "No self link exists. Link and payload will not be populated for resource {ResourceType}",
                            resourceEvent.ResourceType);

                        return;
                    }

                    resourceEvent.Href = apiLinkGenerator.CreateUrl(selfLink, resourceEvent.SelfQuery);

                    // If we do not already have Data use the "SelfQuery" to populate using a nested query
                    if (resourceEvent.Data == null)
                    {
                        await PopulateResourceEventData(resourceEventRepository, context, resourceEvent);
                    }

                    await resourceEventRepository.AddAsync(resourceEvent);
                }
            }
        }

        private static async Task PopulateResourceEventData(
            IResourceEventRepository resourceEventRepository,
            ApiOperationContext context,
            ResourceEvent resourceEvent)
        {
            // Get the latest after creation or update (cannot, obviously, get for a deleted record)
            if (resourceEvent.ChangeType != ResourceEventChangeType.Deleted)
            {
                resourceEvent.Data = await GetByIdAsync(context, resourceEvent.SelfQuery);
            }

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

        private static async Task<object> GetByIdAsync(ApiOperationContext context, object operation)
        {
            var operationType = operation.GetType();
            var nestedContext = context.CreateNested(operation);

            // We are making an assumption here that is must be OK to execute the GetById of the resource that
            // has just been created (otherwise, how was it created?!). This allows the GetById to work on things
            // such as signup commands without manually setting new auth context
            nestedContext.SkipAuthorisation = true;

            var executor = context.ServiceProvider.GetRequiredService<IApiOperationExecutor>();

            var result = await executor.ExecuteAsync(nestedContext);

            if (result is ValidationFailedOperationResult validationFailedResult)
            {
                throw new ValidationException($"GetById for operation {operationType.Name} validation failed", validationFailedResult.Errors);
            }

            if (result is UnhandledExceptionOperationResult exceptionOperationResult)
            {
                exceptionOperationResult.Rethrow();

                return default;
            }

            if (result is OkResult okResult)
            {
                return okResult.Content;
            }

            throw new InvalidOperationException($"Operation {operationType.Name} returned unexpected result type {result.GetType().Name}");
        }
    }
}
