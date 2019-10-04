using Blueprint.Compiler.Frames;

namespace Blueprint.Api
{
    /// <summary>
    /// Provides a simple base class for implementing custom <see cref="IMiddlewareBuilder"/>s that
    /// require a single custom <see cref="Frame" /> to be appended to the generated method.
    /// </summary>
    public abstract class CustomFrameMiddlewareBuilder : Frame, IMiddlewareBuilder
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CustomFrameMiddlewareBuilder" /> class.
        /// </summary>
        /// <param name="isAsync">Whether this frame has async code within (i.e. depends on await).</param>
        protected CustomFrameMiddlewareBuilder(bool isAsync) : base(isAsync)
        {
        }

        public MiddlewareBuilderContext BuilderContext { get; private set; }

        public abstract bool Matches(ApiOperationDescriptor operation);

        public void Build(MiddlewareBuilderContext context)
        {
            BuilderContext = context;

            context.AppendFrames(this);
        }
    }
}
