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
        /// <param name="apiAuthoriserAggregator">An authorisor aggregrator to check if links are allowed to be shown to a user.</param>
        /// <param name="linkGenerator">A link generator to create the URLs of generated links.</param>
        /// <param name="logger">The logger.</param>
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

            var traceLogEnabled = this._logger.IsEnabled(LogLevel.Trace);
            var links = context.DataModel.GetLinksForResource(linkableResource.GetType());

            if (links.Count == 0)
            {
                if (traceLogEnabled)
                {
                    this._logger.LogTrace(
                        "No links have been registered for linkable resource {0}",
                        linkableResource.GetType());
                }

                return;
            }

            // When trace is enabled, log all links we would be checking, else if debug just the count, otherwise nothing logged here
            if (traceLogEnabled)
            {
                this._logger.LogTrace("Attempting to add {0} links for the linkable resource {1}.", links.Count, linkableResource.GetType());
            }

            foreach (var link in links)
            {
                var entityOperation = link.OperationDescriptor;
                var entityOperationName = entityOperation.OperationType.Name;

                if (!entityOperation.IsExposed)
                {
                    if (traceLogEnabled)
                    {
                        this._logger.LogTrace("Operation not exposed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                var result = await this._apiAuthoriserAggregator.CanShowLinkAsync(context, entityOperation, linkableResource);

                if (result.IsAllowed == false)
                {
                    if (traceLogEnabled)
                    {
                        this._logger.LogTrace("Operation is not allowed to be executed, excluding. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                var url = this._linkGenerator.CreateUrl(link, linkableResource);

                // We could not generate this URL, a placeholder value could not be injected. We therefore skip it
                if (url == null)
                {
                    if (traceLogEnabled)
                    {
                        this._logger.LogTrace("Cannot add link, URL was not generated. operation_type={0}", entityOperationName);
                    }

                    continue;
                }

                if (traceLogEnabled)
                {
                    this._logger.LogTrace("All checks passed. Adding link. operation_type={0}", entityOperationName);
                }

                linkableResource.AddLink(link.Rel, new Link
                {
                    Href = url,
                });
            }
        }
    }
}
