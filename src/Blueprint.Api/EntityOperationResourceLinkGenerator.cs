using System.Threading.Tasks;
using Blueprint.Core;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api
{
    public class EntityOperationResourceLinkGenerator : IResourceLinkGenerator
    {
        private readonly IApiAuthoriserAggregator apiAuthoriserAggregator;
        private readonly IApiLinkGenerator linkGenerator;
        private readonly ILogger<EntityOperationResourceLinkGenerator> logger;

        public EntityOperationResourceLinkGenerator(
            IApiAuthoriserAggregator apiAuthoriserAggregator,
            IApiLinkGenerator linkGenerator,
            ILogger<EntityOperationResourceLinkGenerator> logger)
        {
            Guard.NotNull(nameof(apiAuthoriserAggregator), apiAuthoriserAggregator);
            Guard.NotNull(nameof(linkGenerator), linkGenerator);
            Guard.NotNull(nameof(logger), logger);

            this.apiAuthoriserAggregator = apiAuthoriserAggregator;
            this.linkGenerator = linkGenerator;
            this.logger = logger;
        }

        public async Task AddLinksAsync(IApiLinkGenerator apiLinkGenerator, ApiOperationContext context, ILinkableResource linkableResource)
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
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Operation not exposed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                var result = await apiAuthoriserAggregator.CanShowLinkAsync(context, entityOperation, linkableResource);

                if (result.IsAllowed == false)
                {
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Operation can not be executed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("All checks passed. Adding link. operation_type={0}", entityOperationName);
                }

                linkableResource.AddLink(link.Rel, ConvertResourceDescriptorToLink(link, linkableResource));
            }
        }

        private Link ConvertResourceDescriptorToLink(ApiOperationLink link, object result)
        {
            return new Link
            {
                Href = linkGenerator.CreateUrl(link, result),
            };
        }
    }
}
