using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    public class FormatterMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <inheritdoc />
        /// <returns><c>true</c>.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            context.ExecuteMethod.Frames.Add(new ReturnFrame(typeof(OperationResult)));
        }
    }
}
