using System.Linq;
using Blueprint.Api.Authorisation;
using Blueprint.Core;

namespace Blueprint.Api
{
    /// <summary>
    /// An operation that, when executed, will return the metadata for a given <see cref="ApiDataModel"/>.
    /// </summary>
    [DoNotAuditOperation]
    [DoNotRecordPerformanceMetrics]
    [UnexposedOperation]
    [RootLink("/", Rel = "self")]
    [AllowAnonymous]
    public class RootMetadataOperation : IQuery
    {
    }

    public class RootMetadataOperationHandler : SyncApiOperationHandler<RootMetadataOperation>
    {
        private readonly ApiLinkGenerator linkGenerator;

        public RootMetadataOperationHandler(ApiLinkGenerator linkGenerator)
        {
            Guard.NotNull(nameof(linkGenerator), linkGenerator);

            this.linkGenerator = linkGenerator;
        }

        public override object InvokeSync(RootMetadataOperation operation, ApiOperationContext apiOperationContext)
        {
            var systemResource = new RootResource();

            foreach (var link in apiOperationContext.DataModel.GetRootLinks().Where(l => l.OperationDescriptor.IsExposed))
            {
                systemResource.AddLink(link.Rel, new Link
                {
                    Href = linkGenerator.CreateUrlFromLink(link),
                });
            }

            return systemResource;
        }
    }

    public class RootResource : ApiResource
    {
    }
}
