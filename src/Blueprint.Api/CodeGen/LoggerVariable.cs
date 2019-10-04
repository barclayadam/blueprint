using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.CodeGen
{
    public class LoggerVariable : InjectedField
    {
        private readonly string name;

        public LoggerVariable(string name, ILogger logger)
            : base(typeof(ILogger))
        {
            this.name = name;
            Logger = logger;
        }

        /// <summary>
        /// Gets the <see cref="Logger"/> that would be used at runtime, useful in order
        /// to completely remove logging if the log level is not enabled (which assumes the levels are static at build time).
        /// </summary>
        public ILogger Logger { get; }

        public override string CtorArgDeclaration => $"{typeof(ILoggerFactory).FullNameInCode()} {ArgumentName}Factory";

        public override void WriteAssignment(ISourceWriter writer)
        {
            writer.Write($"{Usage} = {ArgumentName}Factory.{nameof(ILoggerFactory.CreateLogger)}(\"{name}\");");
        }
    }
}
