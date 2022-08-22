using Blueprint.Compiler.Frames;

namespace Blueprint.Http.Middleware;

/// <summary>
/// A middleware that will call out to the <see cref="LinkGeneratorHandler" /> to append links to a result of
/// an operation.
/// </summary>
public class LinkGeneratorMiddlewareBuilder : IMiddlewareBuilder
{
    /// <summary>
    /// Returns <c>true</c>.
    /// </summary>
    public bool SupportsNestedExecution => true;

    /// <inheritdoc/>
    public bool Matches(ApiOperationDescriptor operation)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Build(MiddlewareBuilderContext context)
    {
        context.AppendFrames(new ConditionalFrame(
            (v, _) => v.TryFindVariable(typeof(OperationResult)) != null,
            new MethodCall(typeof(LinkGeneratorHandler), nameof(LinkGeneratorHandler.AddLinksAsync))));
    }
}