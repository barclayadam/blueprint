using System.Collections.Generic;
using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    public class LinkGeneratorMiddlewareBuilder : IMiddlewareBuilder
    {
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var getGenerators = context.VariableFromContainer<IEnumerable<IResourceLinkGenerator>>();

            context.AppendFrames(
                getGenerators,
                new MethodCall(typeof(LinkGeneratorHandler), nameof(LinkGeneratorHandler.AddLinksAsync)));
        }
    }
}
