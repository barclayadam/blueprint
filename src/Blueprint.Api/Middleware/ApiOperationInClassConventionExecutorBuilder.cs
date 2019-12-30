using System.Reflection;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilder" /> that will use a public method that exists on the <see cref="IApiOperation" /> class
    /// itself.
    /// </summary>
    public class ApiOperationInClassConventionExecutorBuilder : IOperationExecutorBuilder
    {
        private readonly MethodInfo method;

        /// <summary>
        /// Creates a new instance of the <see cref="ApiOperationInClassConventionExecutorBuilder" /> that represents the given <see cref="ApiOperationDescriptor"/>.
        /// </summary>
        /// <param name="operation">The operation this builder handles.</param>
        /// <param name="method">The method that is to be executed.</param>
        public ApiOperationInClassConventionExecutorBuilder(ApiOperationDescriptor operation, MethodInfo method)
        {
            Operation = operation;
            this.method = method;
        }

        /// <inheritdoc />
        public ApiOperationDescriptor Operation { get; }

        /// <inheritdoc />
        public Variable Build(MiddlewareBuilderContext context)
        {
            // We rely on the compiler infrastructure to make the correct calls, to the correct type (i.e. the actual
            // operation), and to fill in the parameters of that method as required.
            var handlerInvokeCall = new MethodCall(context.Descriptor.OperationType, method);

            context.AppendFrames(
                LogFrame.Debug($"Executing API operation. handler_type={method.DeclaringType.Name}"),
                handlerInvokeCall);

            return handlerInvokeCall.ReturnVariable;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{method.DeclaringType.Name}.{method.Name} handles {Operation}";
        }
    }
}
