using System;
using Blueprint.Auditing;
using Blueprint.Authorisation;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Errors;
using Blueprint.Utilities;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A frame that is output when catching all unhandled exceptions, logging as much information about
    /// the operation, request, exception and user through the use of <see cref="IErrorLogger" />.
    /// </summary>
    public class PushToErrorLoggerExceptionCatchFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext context;

        private readonly Variable exceptionVariable;

        /// <summary>
        /// Initialises a new instance of the <see cref="PushToErrorLoggerExceptionCatchFrame" /> class.
        /// </summary>
        /// <param name="context">The builder context this frame belongs to.</param>
        /// <param name="exceptionVariable">The variable representing the <see cref="Exception" /> that has
        /// been raised.</param>
        public PushToErrorLoggerExceptionCatchFrame(MiddlewareBuilderContext context, Variable exceptionVariable)
        {
            this.context = context;
            this.exceptionVariable = exceptionVariable;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var contextVariable = variables.FindVariable(typeof(ApiOperationContext));

            writer.WriteLine($"var userAuthorisationContext = {contextVariable}.UserAuthorisationContext;");
            writer.WriteLine($"var identifier = new {typeof(UserExceptionIdentifier).FullNameInCode()}(userAuthorisationContext);");

            writer.BlankLine();

            // 1. Allow user context to populate metadata to the error data dictionary if it exists
            writer.WriteLine($"userAuthorisationContext?.PopulateMetadata((k, v) => {exceptionVariable}.Data[k] = v?.ToString());");

            var operationTypeKey = ReflectionUtilities.PrettyTypeName(context.Descriptor.OperationType);

            // 2. For every property of the operation output a value to the exception.Data dictionary. ALl properties that are
            // not considered sensitive
            foreach (var prop in context.Descriptor.Properties)
            {
                if (SensitiveProperties.IsSensitive(prop))
                {
                    continue;
                }

                // If the type is primitive we need to leave off the '?' null-coalesce method call operator
                var shouldHandleNull = !prop.PropertyType.IsValueType;

                writer.WriteLine($"{exceptionVariable}.Data[\"{operationTypeKey}.{prop.Name}\"] = " +
                                 $"{variables.FindVariable(context.Descriptor.OperationType)}.{prop.Name}{(shouldHandleNull ? "?" : string.Empty)}.ToString();");
            }

            // 3. Use IErrorLogger to push all details to exception sinks
            writer.BlankLine();

            // We use an inline MethodCall here to enable it to ensure surrounding method is marked as async as necessary
            var methodCall = MethodCall.For<IErrorLogger>(e => e.LogAsync(default(Exception), default(object), default(UserExceptionIdentifier)));
            methodCall.Arguments[0] = exceptionVariable;
            methodCall.Arguments[1] = new Variable(typeof(object), "null");
            methodCall.Arguments[2] = new Variable(typeof(UserExceptionIdentifier), "identifier");

            writer.Write(methodCall);

            writer.BlankLine();
        }
    }
}
