using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Api.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Middleware
{
    public static class LinkGeneratorHandler
    {
        public static async Task AddLinksAsync(IEnumerable<IResourceLinkGenerator> registeredGenerators, ApiOperationContext context, OperationResult result)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<LinkGeneratorMiddlewareBuilder>>();
            var generators = registeredGenerators.ToList();

            if (result is OkResult okResult)
            {
                var innerResult = okResult.Content;

                if (innerResult is ResourceEvent resourceEvent)
                {
                    if (resourceEvent.ChangeType == ResourceEventChangeType.Deleted)
                    {
                        // We will not generate any links for a deleted resource. The payload is there more
                        // for convenience to identify what has been deleted. Even self links do not make sense
                        // as the resource no longer exists
                        logger.LogTrace("No links being generated for a deleted ResourceEvent");
                    }
                    else if (resourceEvent.Data != null)
                    {
                        logger.LogTrace("Adding links to ResourceEvent.Data");

                        resourceEvent.Data = await AddResourceLinksAsync(logger, generators, context, resourceEvent.Data);
                    }
                }
                else
                {
                    await AddResourceLinksAsync(logger, generators, context, innerResult);
                }
            }
        }

        private static async Task<object> AddResourceLinksAsync(
            ILogger<LinkGeneratorMiddlewareBuilder> logger,
            List<IResourceLinkGenerator> generators,
            ApiOperationContext context,
            object resource)
        {
            if (resource is ApiResource apiResource)
            {
                await AddLinksAsync(logger, generators, context, apiResource);
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Resource type is not a LinkableResource. resource_type={0}", resource.GetType().Name);
            }

            var enumerableResult = resource as IEnumerable<object>;

            if (resource is IPagedApiResource pagedResult)
            {
                enumerableResult = pagedResult.GetEnumerable();
            }

            if (enumerableResult != null)
            {
                if (enumerableResult is IQueryable<object>)
                {
                    // We need to ensure we are now dealing with a non-deferred result, else the
                    // links will be added, but the deferred result is still returned and so
                    // changes are lost.
                    //
                    // This needs to be what we then actually return from the middleware after the
                    // links have been added
                    enumerableResult = enumerableResult.ToList();
                    resource = enumerableResult;
                }

                foreach (var obj in enumerableResult)
                {
                    if (obj is ApiResource apiResourceItem)
                    {
                        await AddLinksAsync(logger, generators, context, apiResourceItem);
                    }
                    else
                    {
                        // If we cannot add any links because not a `LinkableResource` then break early
                        // as we assume all entries are the same type
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Resource type is not a LinkableResource. resource_type={0}", obj.GetType().Name);
                        }

                        break;
                    }
                }
            }

            return resource;
        }

        private static async Task AddLinksAsync(ILogger<LinkGeneratorMiddlewareBuilder> logger, List<IResourceLinkGenerator> generators, ApiOperationContext context, ILinkableResource result)
        {
            foreach (var resourceLinkGenerator in generators)
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Generating links. generator={0} result_type={1}", resourceLinkGenerator.GetType().Name, result.GetType().Name);
                }

                await resourceLinkGenerator.AddLinksAsync(context, result);
            }
        }
    }
}
