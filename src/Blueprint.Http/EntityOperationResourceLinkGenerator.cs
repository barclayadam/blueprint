using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Blueprint.Http
{
    public class EntityOperationResourceLinkGenerator : IResourceLinkGenerator
    {
        private readonly IApiAuthoriserAggregator _apiAuthoriserAggregator;
        private readonly IApiLinkGenerator _linkGenerator;
        private readonly ILogger<EntityOperationResourceLinkGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOperationResourceLinkGenerator"/> class.
        /// </summary>
        /// <param name="apiAuthoriserAggregator"></param>
        /// <param name="linkGenerator"></param>
        /// <param name="logger"></param>
        public EntityOperationResourceLinkGenerator(
            IApiAuthoriserAggregator apiAuthoriserAggregator,
            IApiLinkGenerator linkGenerator,
            ILogger<EntityOperationResourceLinkGenerator> logger)
        {
            Guard.NotNull(nameof(apiAuthoriserAggregator), apiAuthoriserAggregator);
            Guard.NotNull(nameof(linkGenerator), linkGenerator);
            Guard.NotNull(nameof(logger), logger);

            this._apiAuthoriserAggregator = apiAuthoriserAggregator;
            this._linkGenerator = linkGenerator;
            this._logger = logger;
        }

        /// <inheritdoc/>
        public async ValueTask AddLinksAsync(IApiLinkGenerator apiLinkGenerator, ApiOperationContext context, ILinkableResource linkableResource)
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
                    if (this._logger.IsEnabled(LogLevel.Trace))
                    {
                        this._logger.LogTrace("Operation not exposed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                var result = await this._apiAuthoriserAggregator.CanShowLinkAsync(context, entityOperation, linkableResource);

                if (result.IsAllowed == false)
                {
                    if (this._logger.IsEnabled(LogLevel.Trace))
                    {
                        this._logger.LogTrace("Operation can not be executed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                if (this._logger.IsEnabled(LogLevel.Trace))
                {
                    this._logger.LogTrace("All checks passed. Adding link. operation_type={0}", entityOperationName);
                }

                linkableResource.AddLink(link.Rel, new Link
                {
                    Href = this._linkGenerator.CreateUrl(link, linkableResource),
                });
            }
        }
    }
}
