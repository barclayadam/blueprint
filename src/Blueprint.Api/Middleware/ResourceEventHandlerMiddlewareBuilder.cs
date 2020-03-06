﻿using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    public class ResourceEventHandlerMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool SupportsNestedExecution => true;

        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.IsCommand;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var getResourceEventRepository = context.VariableFromContainer<IResourceEventRepository>();
            var getApiLinkGenerator = context.VariableFromContainer<IApiLinkGenerator>();

            context.AppendFrames(
                getResourceEventRepository,
                getApiLinkGenerator,
                new MethodCall(typeof(ResourceEventHandler), nameof(ResourceEventHandler.HandleAsync)));
        }
    }
}
