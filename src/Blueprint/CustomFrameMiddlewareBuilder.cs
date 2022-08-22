using Blueprint.Compiler.Frames;

namespace Blueprint;

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
    protected CustomFrameMiddlewareBuilder(bool isAsync)
        : base(isAsync)
    {
    }

    /// <inheritdoc />
    public abstract bool SupportsNestedExecution { get; }

    /// <summary>
    /// Gets or sets the <see cref="MiddlewareBuilderContext" /> this builder is being built for.
    /// </summary>
    protected MiddlewareBuilderContext BuilderContext { get; private set; }

    /// <inheritdoc />
    public abstract bool Matches(ApiOperationDescriptor operation);

    /// <inheritdoc />
    public void Build(MiddlewareBuilderContext context)
    {
        this.BuilderContext = context;

        context.AppendFrames(this);
    }
}