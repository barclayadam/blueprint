using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using NLog;

namespace Blueprint.Api.CodeGen
{
    public class LoggerVariable : StaticField
    {
        public LoggerVariable(Logger logger)
            : base(typeof(Logger), $"{typeof(LogManager).FullNameInCode()}.{nameof(LogManager.GetLogger)}(\"{logger.Name}\")")
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the <see cref="Logger"/> that would be used at runtime, useful in order
        /// to completely remove logging if the log level is not enabled (which assumes the levels are static at build time).
        /// </summary>
        public Logger Logger { get; }
    }
}
