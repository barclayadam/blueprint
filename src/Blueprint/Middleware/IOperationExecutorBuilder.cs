using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Middleware
{
    /// <summary>
    /// A "builder" for adding the code necessary to an API pipeline that will perform the actual execution of
    /// the operation.
    /// </summary>
    /// <see cref="ApiOperationHandlerExecutorBuilder"/>
    public interface IOperationExecutorBuilder
    {
        /// <summary>
        /// Gets the name of this builder, useful for diagnostics and tracing and should be the unique
        /// handler implementation (i.e. the actual IApiOperationHandler or equivalent).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Performs the code generation for this builder, adding <see cref="Frame"/>s to call the appropriate
        /// handler and return the <see cref="Variable" /> that represents that execution.
        /// </summary>
        /// <param name="context">The context of the currently built pipeline.</param>
        /// <param name="executorReturnType">The return type this builder needs to follow.</param>
        /// <returns>The variable that represents the result of executing a handler for the operation.</returns>
        Variable Build(MiddlewareBuilderContext context, ExecutorReturnType executorReturnType);
    }
}
