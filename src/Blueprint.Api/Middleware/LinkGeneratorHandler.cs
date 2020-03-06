﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Middleware
{
    public static class LinkGeneratorHandler
    {
        public static async Task AddLinksAsync(
            IApiLinkGenerator apiLinkGenerator,
            IEnumerable<IResourceLinkGenerator> registeredGenerators,
            ApiOperationContext context,
            OperationResult result)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<LinkGeneratorMiddlewareBuilder>>();
            var generators = registeredGenerators.ToList();

            if (result is OkResult okResult)
            {
                okResult.Content = await AddResourceLinksAsync(logger, apiLinkGenerator, generators, context, okResult.Content);
            }
        }

        private static async Task<object> AddResourceLinksAsync(
            ILogger<LinkGeneratorMiddlewareBuilder> logger,
            IApiLinkGenerator apiLinkGenerator,
            List<IResourceLinkGenerator> generators,
            ApiOperationContext context,
            object resource)
        {
            if (resource is ILinkableResource linkableResource)
            {
                await AddLinksAsync(logger, apiLinkGenerator, generators, context, linkableResource);
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Resource type is not an ILinkableResource. resource_type={0}", resource.GetType().Name);
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
                        await AddLinksAsync(logger, apiLinkGenerator, generators, context, apiResourceItem);
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

        private static async Task AddLinksAsync(
            ILogger<LinkGeneratorMiddlewareBuilder> logger,
            IApiLinkGenerator apiLinkGenerator,
            List<IResourceLinkGenerator> generators,
            ApiOperationContext context,
            ILinkableResource result)
        {
            foreach (var resourceLinkGenerator in generators)
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Generating links. generator={0} result_type={1}", resourceLinkGenerator.GetType().Name, result.GetType().Name);
                }

                await resourceLinkGenerator.AddLinksAsync(apiLinkGenerator, context, result);
            }
        }
    }
}
