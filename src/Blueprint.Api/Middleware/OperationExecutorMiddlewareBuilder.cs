using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An <see cref="IMiddlewareBuilder"/> that will be last in the chain for a pipeline that will call out to a
    /// new instance (from registered container) of <see cref="IApiOperationHandler{T}"/> for the current operation.
    /// </summary>
    public class OperationExecutorMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <inheritdoc />
        /// <returns>true</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var handlerType = typeof(IApiOperationHandler<>).MakeGenericType(context.Descriptor.OperationType);

            var getInstanceFrame = context.VariableFromContainer(handlerType);
            var handlerInvokeCall = new MethodCall(handlerType, nameof(IApiOperationHandler<IApiOperation>.Invoke));
            handlerInvokeCall.ReturnVariable.OverrideName("handlerResult");

            context.AppendFrames(
                getInstanceFrame,

                LogFrame.Debug("Executing API operation. handler_type={0}", $"{getInstanceFrame.InstanceVariable}.GetType().Name"),

                handlerInvokeCall,

                new OperationResultCastFrame(handlerInvokeCall.ReturnVariable)
            );
        }

        /// <summary>
        /// A <see cref="SyncFrame" /> that will, given the result from executing the individual handler for an operation,
        /// ensure it is of type <see cref="OperationResult" /> (wrapping anything that is not with <see cref="OkResult"/>).
        /// </summary>
        private class OperationResultCastFrame : SyncFrame
        {
            private readonly Variable resultVariable;
            private readonly Variable operationResultVariable;

            public OperationResultCastFrame(Variable resultVariable)
            {
                this.resultVariable = resultVariable;

                operationResultVariable = new Variable(typeof(OperationResult), this);
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                var operationResultName = typeof(OperationResult).FullNameInCode();
                var okResultName = typeof(OkResult).FullNameInCode();

                writer.Write($"{operationResultName} {operationResultVariable} = {resultVariable} is {operationResultName} r ? " +
                              "r : " +
                             $"new {okResultName}({resultVariable});");

                Next?.GenerateCode(method, writer);
            }
        }
    }
}
