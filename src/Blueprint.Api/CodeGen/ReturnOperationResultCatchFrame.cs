using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Errors;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A frame that is output when catching all unhandled exceptions, logging as much information about
    /// the operation, request, exception and user through the use of <see cref="IErrorLogger" />.
    /// </summary>
    public class ReturnOperationResultCatchFrame : SyncFrame
    {
        private readonly Variable exceptionVariable;

        public ReturnOperationResultCatchFrame(Variable exceptionVariable)
        {
            this.exceptionVariable = exceptionVariable;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            // 4. Return UnhandledExceptionOperationResult that will attempt to determine best status code etc. depending
            // on exception type.
            writer.Write($"return new {typeof(UnhandledExceptionOperationResult).FullNameInCode()}({exceptionVariable});");
        }
    }
}
