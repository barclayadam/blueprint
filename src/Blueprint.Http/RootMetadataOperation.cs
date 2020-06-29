using Blueprint.Authorisation;

namespace Blueprint.Http
{
    /// <summary>
    /// An operation that, when executed, will return the metadata for a given <see cref="ApiDataModel"/> in the
    /// form of an empty <see cref="RootResource" /> with links for all registered operations.
    /// </summary>
    [DoNotAuditOperation]
    [DoNotRecordPerformanceMetrics]
    [UnexposedOperation]
    [RootLink("/", Rel = "self")]
    [AllowAnonymous]
    public class RootMetadataOperation : IQuery<RootResource>
    {
        /// <summary>
        /// Invokes this query, returning a simple model of available routes within this API.
        /// </summary>
        /// <param name="linkGenerator">The API link generator.</param>
        /// <param name="dataModel">The API data model.</param>
        /// <returns>A <see cref="RootResource" />.</returns>
        public RootResource Invoke(IApiLinkGenerator linkGenerator, ApiDataModel dataModel)
        {
            var systemResource = new RootResource();

            foreach (var link in dataModel.GetRootLinks())
            {
                if (link.OperationDescriptor.IsExposed)
                {
                    systemResource.AddLink(link.Rel, new Link
                    {
                        Href = linkGenerator.CreateUrl(link),
                    });
                }
            }

            return systemResource;
        }
    }

    /// <summary>
    /// A "root" resource, as returned from <see cref="RootMetadataOperation" />.
    /// </summary>
    public class RootResource : ApiResource
    {
    }
}
