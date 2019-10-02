using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Blueprint.Api.Middleware
{
    public static class LinkGeneratorHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task AddLinksAsync(IEnumerable<IResourceLinkGenerator> registeredGenerators, ApiOperationContext context, OperationResult result)
        {
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
                        Log.Trace("No links being generated for a deleted ResourceEvent");
                    }
                    else if (resourceEvent.Data != null)
                    {
                        Log.Trace("Adding links to ResourceEvent.Data");

                        resourceEvent.Data = await AddResourceLinksAsync(generators, context, resourceEvent.Data);
                    }
                }
                else
                {
                    await AddResourceLinksAsync(generators, context, innerResult);
                }
            }
        }

        private static async Task<object> AddResourceLinksAsync(
            List<IResourceLinkGenerator> generators,
            ApiOperationContext context,
            object resource)
        {
            if (resource is ApiResource apiResource1)
            {
                await AddLinksAsync(generators, context, apiResource1);
            }

            if (Log.IsTraceEnabled)
            {
                Log.Trace("Resource type is not a LinkableResource. resource_type={0}", resource.GetType().Name);
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
                    if (obj is ApiResource apiResource)
                    {
                        await AddLinksAsync(generators, context, apiResource);
                    }
                    else
                    {
                        // If we cannot add any links because not a `LinkableResource` then break early
                        // as we assume all entries are the same type
                        if (Log.IsTraceEnabled)
                        {
                            Log.Trace("Resource type is not a LinkableResource. resource_type={0}", obj.GetType().Name);
                        }

                        break;
                    }
                }
            }

            return resource;
        }

        private static async Task AddLinksAsync(List<IResourceLinkGenerator> generators, ApiOperationContext context, ILinkableResource result)
        {
            foreach (var resourceLinkGenerator in generators)
            {
                if (Log.IsTraceEnabled)
                {
                    Log.Trace("Generating links. generator={0} result_type={1}", resourceLinkGenerator.GetType().Name, result.GetType().Name);
                }

                await resourceLinkGenerator.AddLinksAsync(context, result);
            }
        }
    }
}
