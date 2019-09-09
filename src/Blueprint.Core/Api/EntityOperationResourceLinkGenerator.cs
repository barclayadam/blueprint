using System.Threading.Tasks;

using NLog;

namespace Blueprint.Core.Api
{
    public class EntityOperationResourceLinkGenerator : IResourceLinkGenerator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IApiAuthoriserAggregator apiAuthoriserAggregator;
        private readonly ApiLinkGenerator linkGenerator;

        public EntityOperationResourceLinkGenerator(IApiAuthoriserAggregator apiAuthoriserAggregator, ApiLinkGenerator linkGenerator)
        {
            Guard.NotNull(nameof(apiAuthoriserAggregator), apiAuthoriserAggregator);
            Guard.NotNull(nameof(linkGenerator), linkGenerator);

            this.apiAuthoriserAggregator = apiAuthoriserAggregator;
            this.linkGenerator = linkGenerator;
        }

        public async Task AddLinksAsync(ApiOperationContext context, ILinkableResource linkableResource)
        {
            Guard.NotNull(nameof(context), context);
            Guard.NotNull(nameof(linkableResource), linkableResource);

            var links = context.DataModel.GetLinksForResource(linkableResource.GetType());

            foreach (var link in links)
            {
                var entityOperation = link.OperationDescriptor;
                var entityOperationName = entityOperation.OperationType.Name;

                if (!entityOperation.IsExposed)
                {
                    if (Log.IsTraceEnabled)
                    {
                        Log.Trace("Operation not exposed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                var result = await apiAuthoriserAggregator.CanShowLinkAsync(context, entityOperation, linkableResource);

                if (result.IsAllowed == false)
                {
                    if (Log.IsTraceEnabled)
                    {
                        Log.Trace("Operation can not be executed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                if (Log.IsTraceEnabled)
                {
                    Log.Trace("All checks passed. Adding link. operation_type={0}", entityOperationName);
                }

                linkableResource.AddLink(link.Rel, ConvertResourceDescriptorToLink(link, linkableResource));
            }
        }

        private Link ConvertResourceDescriptorToLink(ApiOperationLink link, object result)
        {
            return new Link
            {
                Href = linkGenerator.CreateUrlFromLink(link, result)
            };
        }
    }
}