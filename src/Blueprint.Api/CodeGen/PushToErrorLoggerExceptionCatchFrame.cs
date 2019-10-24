using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Errors;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A frame that is output when catching all unhandled exceptions, logging as much information about
    /// the operation, request, exception and user through the use of <see cref="IErrorLogger" />.
    /// </summary>
    public class PushToErrorLoggerExceptionCatchFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext context;

        private readonly Variable exceptionVariable;
        private readonly GetInstanceFrame<IErrorLogger> getErrorLoggerFrame;

        private Variable contextVariable;

        public PushToErrorLoggerExceptionCatchFrame(MiddlewareBuilderContext context, Variable exceptionVariable)
        {
            this.context = context;
            this.exceptionVariable = exceptionVariable;

            getErrorLoggerFrame = context.VariableFromContainer<IErrorLogger>();
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            getErrorLoggerFrame.GenerateCode(method, writer);

            writer.Write($"var userAuthorisationContext = {contextVariable}.UserAuthorisationContext;");
            writer.Write($"var identifier = new {typeof(UserExceptionIdentifier).FullNameInCode()}(userAuthorisationContext);");

            writer.BlankLine();

            // 1. Allow user context to populate metadata to the error data dictionary if it exists
            writer.Write($"userAuthorisationContext?.PopulateMetadata((k, v) => {exceptionVariable}.Data[k] = v?.ToString());");

            // 2. For every property of the operation output a value to the exception.Data dictionary. ALl properties that are
            // not considered sensitive
            foreach (var prop in context.Descriptor.Properties)
            {
                if (SensitiveProperties.IsSensitive(prop))
                {
                    continue;
                }

                // If the type is primitive we need to leave off the '?' null-coalesce method call operator
                var shouldHandleNull = prop.PropertyType.IsClass;

                writer.Write($"{exceptionVariable}.Data[\"{context.Descriptor.OperationType.Name}.{prop.Name}\"] = " +
                             $"{context.ApiContextVariableSource.OperationVariable}.{prop.Name}{(shouldHandleNull ? "?" : string.Empty)}.ToString();");
            }

            // 3. Use IErrorLogger to push all details to exception sinks
            writer.BlankLine();
            writer.Write($"{getErrorLoggerFrame.InstanceVariable}.Log({exceptionVariable}, {contextVariable}.HttpContext, identifier);");
            writer.BlankLine();
        }

        /// <inheritdoc />
        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            contextVariable = chain.FindVariable(typeof(ApiOperationContext));

            yield return contextVariable;

            foreach (var v in getErrorLoggerFrame.FindVariables(chain))
            {
                yield return v;
            }
        }
    }
}
