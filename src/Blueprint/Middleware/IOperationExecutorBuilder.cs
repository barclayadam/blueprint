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
        /// Gets the <see cref="ApiOperationDescriptor" /> that this builder has been registered for.
        /// </summary>
        ApiOperationDescriptor Operation { get; }

        /// <summary>
        /// Performs the code generation for this builder, adding <see cref="Frame"/>s to call the appropriate
        /// handler and return the <see cref="Variable" /> that represents that execution.
        /// </summary>
        /// <param name="context">The context of the currently built pipeline (will represent the same operation as <see cref="Operation"/>.</param>
        /// <returns>The variable that represents the result of executing a handler for the operation.</returns>
        Variable Build(MiddlewareBuilderContext context);
    }
}
