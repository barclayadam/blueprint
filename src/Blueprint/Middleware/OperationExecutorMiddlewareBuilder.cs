using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

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
            var allHandlers = context.ServiceProvider.GetRequiredService<List<IOperationExecutorBuilder>>();
            var handler = allHandlers.Single(h => h.Operation == context.Descriptor);

            var returnVariable = handler.Build(context);
            returnVariable.OverrideName("handlerResult");

            context.AppendFrames(new OperationResultCastFrame(returnVariable));
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

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationResultName = typeof(OperationResult).FullNameInCode();

                if (typeof(OperationResult).IsAssignableFrom(resultVariable.VariableType))
                {
                    // If the declared type is already a type of OperationResult we can eliminate the ternary operator check and
                    // immediately assign to the operationResult variable
                    writer.Write($"{operationResultName} {operationResultVariable} = {resultVariable};");
                }
                else if (resultVariable.VariableType.IsAssignableFrom(typeof(OperationResult)))
                {
                    // If the variable type _could_ be an OperationResult then we use a ternary operator to check whether it
                    // actually is, and either use it directly or wrap in an OkResult
                    var okResultName = typeof(OkResult).FullNameInCode();

                    writer.Write($"{operationResultName} {operationResultVariable} = {resultVariable} is {operationResultName} r ? " +
                                 "r : " +
                                 $"new {okResultName}({resultVariable});");
                }
                else
                {
                    // The type is NOT related to OperationResult at all, so we always create a wrapping OkResult
                    var okResultName = typeof(OkResult).FullNameInCode();

                    writer.Write($"{operationResultName} {operationResultVariable} = new {okResultName}({resultVariable});");
                }

                next();
            }
        }
    }
}
