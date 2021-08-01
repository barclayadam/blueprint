using System;
using System.Diagnostics;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Diagnostics;

namespace Blueprint.Middleware
{
    /// <summary>
    /// An <see cref="IMiddlewareBuilder"/> that will find the registered <see cref="IOperationExecutorBuilder" /> for the operation being
    /// built for, delegating the actual code generation of calling the correct handler for the operation.
    /// </summary>
    public class OperationExecutorMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool SupportsNestedExecution => true;

        /// <inheritdoc />
        /// <returns>true.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            if (context.Descriptor.Handlers.Count > 1)
            {
                BuildForMultipleHandlers(context);
            }
            else
            {
                BuildForSingleHandler(context);
            }
        }

        private static void BuildForSingleHandler(MiddlewareBuilderContext context)
        {
            // We only have a single handler, return the result of that
            var allHandlers = context.Descriptor.Handlers;
            var handler = allHandlers.Single();

            var apmFrame = ActivityFrame.Start(ActivityKind.Internal, handler.HandlerType.NameInCode());
            context.AppendFrames(apmFrame);

            var returnVariable = handler.Build(context, ExecutorReturnType.Return);

            if (returnVariable == null && context.Descriptor.RequiresReturnValue)
            {
                throw new InvalidOperationException(
                    $@"Unable to build an executor for the operation {context.Descriptor} because the single handler registered, {handler}, did not return a variable but the operation has {nameof(context.Descriptor.RequiresReturnValue)} set to true. 

This can happen if an the only registered handler for an operation is one that is NOT of the same type (for example a handler IApiOperationHandler<ConcreteClass> for the operation IOperationInterface) where it cannot be guaranteed that the handler will be executed.");
            }

            if (returnVariable != null)
            {
                returnVariable.OverrideName("handlerResult");

                context.AppendFrames(new OperationResultCastFrame(returnVariable));
            }

            context.AppendFrames(apmFrame.Complete());
        }

        private static void BuildForMultipleHandlers(MiddlewareBuilderContext context)
        {

            if (context.Descriptor.RequiresReturnValue)
            {
                throw new InvalidOperationException(
                    $@"Unable to build an executor for the operation {context.Descriptor} because multiple handlers have been registered but the operation has {nameof(context.Descriptor.RequiresReturnValue)} set to true. 

When {nameof(context.Descriptor.RequiresReturnValue)} is true multiple handlers cannot be used as there is not one, obvious, return value that could be used.

Handlers found:
  - {string.Join("\n  - ", context.Descriptor.Handlers)}
");
            }

            foreach (var handler in context.Descriptor.Handlers)
            {
                var apmFrame = ActivityFrame.Start(ActivityKind.Internal, handler.HandlerType.NameInCode());
                context.AppendFrames(apmFrame);

                handler.Build(context, ExecutorReturnType.NoReturn);

                context.AppendFrames(apmFrame.Complete());
            }
        }

        /// <summary>
        /// A <see cref="SyncFrame" /> that will, given the result from executing the individual handler for an operation,
        /// ensure it is of type <see cref="OperationResult" /> (wrapping anything that is not with <see cref="OkResult"/>).
        /// </summary>
        private class OperationResultCastFrame : SyncFrame
        {
            private readonly Variable _resultVariable;
            private readonly Variable _operationResultVariable;

            public OperationResultCastFrame(Variable resultVariable)
            {
                this._resultVariable = resultVariable;

                this._operationResultVariable = new Variable(typeof(OperationResult), this);
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationResultName = typeof(OperationResult).FullNameInCode();

                if (typeof(OperationResult).IsAssignableFrom(this._resultVariable.VariableType))
                {
                    // If the declared type is already a type of OperationResult we can eliminate the ternary operator check and
                    // immediately assign to the operationResult variable
                    writer.WriteLine($"{operationResultName} {this._operationResultVariable} = {this._resultVariable};");
                }
                else if (this._resultVariable.VariableType.IsAssignableFrom(typeof(OperationResult)))
                {
                    // If the variable type _could_ be an OperationResult then we use a ternary operator to check whether it
                    // actually is, and either use it directly or wrap in an OkResult
                    var okResultName = typeof(OkResult).FullNameInCode();

                    writer.WriteLine($"{operationResultName} {this._operationResultVariable} = {this._resultVariable} is {operationResultName} r ? " +
                                     "r : " +
                                     $"new {okResultName}({this._resultVariable});");
                }
                else
                {
                    // The type is NOT related to OperationResult at all, so we always create a wrapping OkResult
                    var okResultName = typeof(OkResult).FullNameInCode();

                    writer.WriteLine($"{operationResultName} {this._operationResultVariable} = new {okResultName}({this._resultVariable});");
                }

                next();
            }
        }
    }
}
