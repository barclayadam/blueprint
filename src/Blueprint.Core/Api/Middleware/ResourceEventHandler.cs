using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Blueprint.Core.Tracing;

using NLog;

namespace Blueprint.Core.Api.Middleware
{
    /// <summary>
    /// A middleware component that will take any <see cref="ResourceEvent" /> objects returned from
    /// API operations and populate the return with extra information such as the resource itself
    /// an it's canonical 'self' link for consumption elsewhere.
    /// </summary>
    public static class ResourceEventHandler
    {
        private static readonly MethodInfo GetByIdMethod =
            typeof(ResourceEventHandler).GetMethod(nameof(GetById), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task HandleAsync(
            IResourceEventRepository resourceEventRepository,
            ApiLinkGenerator apiLinkGenerator,
            ApiOperationContext context,
            OperationResult result)
        {
            if (result is OkResult okResult)
            {
                var innerResult = okResult.Content;

                if (innerResult is ResourceEvent resourceEvent)
                {
                    Log.Debug("ResourceEvent found. Loading resource. resource_type={0}", resourceEvent.ResourceType);

                    context.UserAuthorisationContext.PopulateMetadata((k,v) => resourceEvent.Metadata[k] = v);

                    resourceEvent.CorrelationId = Activity.Current.Id;
                    resourceEvent.Operation = context.Operation;

                    PopulateClientData(context, resourceEvent);

                    var selfLink = context.DataModel.GetLinkFor(resourceEvent.ResourceType, "self");

                    if (selfLink == null)
                    {
                        Log.Warn("No self link exists. Link and payload will not be populated. resource_type={0}",
                            resourceEvent.ResourceType.Name);

                        return;
                    }

                    resourceEvent.Href = apiLinkGenerator.CreateUrlFromLink(selfLink, resourceEvent.SelfQuery);

                    await PopulateResourceEventData(resourceEventRepository, context, resourceEvent, selfLink);

                    await resourceEventRepository.AddAsync(resourceEvent);
                }
            }
        }

        private static void PopulateClientData(ApiOperationContext context, ResourceEvent resourceEvent)
        {
            if (context.Request == null)
            {
                return;
            }

            resourceEvent.Metadata.Add("IpAddress", context.Request.GetClientIpAddress());

            if (context.Request.Headers.ContainsKey("User-Agent"))
            {
                resourceEvent.Metadata.Add("UserAgent",
                    string.Join(" ", context.Request.Headers["User-Agent"].ToString()));
            }
        }

        private static async Task PopulateResourceEventData(
            IResourceEventRepository resourceEventRepository,
            ApiOperationContext context,
            ResourceEvent resourceEvent,
            ApiOperationLink selfLink)
        {
            // Get the latest after creation or update (cannot, obviously, get for a deleted record)
            if (resourceEvent.ChangeType != ResourceEventChangeType.Deleted)
            {
                resourceEvent.Data = await (Task<object>)GetByIdMethod
                    .MakeGenericMethod(selfLink.OperationDescriptor.OperationType)
                    .Invoke(null, new object[] { context, resourceEvent.SelfQuery });
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

        private static Task<object> GetById<T>(ApiOperationContext context, T operation) where T : IApiOperation
        {
            var handler = context.Container.GetInstance<IApiOperationHandler<T>>();

            return handler.Invoke(operation, context);
        }
    }
}
