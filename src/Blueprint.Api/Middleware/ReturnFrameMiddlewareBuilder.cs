using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// The last <see cref="IMiddlewareBuilder" /> that is solely responsible for returning
    /// the result of the pipeline (i.e. the <see cref="OperationResult" /> that has been created).
    /// </summary>
    public class ReturnFrameMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool SupportsNestedExecution => true;

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
