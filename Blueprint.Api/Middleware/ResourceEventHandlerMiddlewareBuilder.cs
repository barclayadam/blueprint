using System.Net.Http;
using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    public class ResourceEventHandlerMiddlewareBuilder : IMiddlewareBuilder
    {
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.HttpMethod != HttpMethod.Get;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var getResourceEventRepository = context.VariableFromContainer<IResourceEventRepository>();
            var getApiLinkGenerator = context.VariableFromContainer<ApiLinkGenerator>();

            context.AppendFrames(
                getResourceEventRepository,
                getApiLinkGenerator,
                new MethodCall(typeof(ResourceEventHandler), nameof(ResourceEventHandler.HandleAsync)));
        }
    }
}
