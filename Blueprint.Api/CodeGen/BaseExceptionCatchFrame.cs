using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Auditing;
using Blueprint.Core.Errors;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A frame that is output when catching all unhandled exceptions, logging as much information about
    /// the operation, request, exception and user through the use of <see cref="IErrorLogger" />.
    /// </summary>
    public class BaseExceptionCatchFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext context;

        private readonly Variable exceptionVariable;
        private readonly GetInstanceFrame<IErrorLogger> getErrorLoggerFrame;

        private Variable contextVariable;

        public BaseExceptionCatchFrame(MiddlewareBuilderContext context, Variable exceptionVariable)
        {
            this.context = context;
            this.exceptionVariable = exceptionVariable;

            getErrorLoggerFrame = context.VariableFromContainer<IErrorLogger>();
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            getErrorLoggerFrame.GenerateCode(method, writer);

            writer.Write($"var errorData = new {typeof(Dictionary<string, string>).FullNameInCode()}();");
            writer.Write($"var userAuthorisationContext = {contextVariable}.UserAuthorisationContext;");
            writer.Write($"var identifier = new {typeof(UserExceptionIdentifier).FullNameInCode()}(userAuthorisationContext);");

            writer.BlankLine();

            // 1. Allow user context to populate metadata to the error data dictionary if it exists
            writer.Write("userAuthorisationContext?.PopulateMetadata((k, v) => errorData[k] = v?.ToString());");

            // 2. For every property of the operation output a value to the errorData dictionary. ALl properties that are
            // not considered sensitive
            foreach (var prop in context.Descriptor.Properties)
            {
                if (SensitiveProperties.IsSensitive(prop))
                {
                    continue;
                }

                // If the type is primitive we need to leave off the '?' null-coalesce method call operator
                var shouldHandleNull = prop.PropertyType.IsClass;

                writer.Write($"errorData[\"{context.Descriptor.Name}.{prop.Name}\"] = " +
                             $"{context.ApiContextVariableSource.OperationVariable}.{prop.Name}{(shouldHandleNull ? "?" : "")}.ToString();");
            }

            // 3. Use IErrorLogger to push all details to exception sinks
            writer.BlankLine();
            writer.Write($"{getErrorLoggerFrame.InstanceVariable}.Log({exceptionVariable}, errorData, {contextVariable}.HttpContext, identifier);");
            writer.BlankLine();

            // 4. Return UnhandledExceptionOperationResult that will attempt to determine best status code etc. depending
            // on exception type.
            writer.Write($"return new {typeof(UnhandledExceptionOperationResult).FullNameInCode()}({exceptionVariable});");
        }

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
