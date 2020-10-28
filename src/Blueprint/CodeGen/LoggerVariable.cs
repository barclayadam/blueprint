using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.CodeGen
{
    public class LoggerVariable : InjectedField
    {
        private readonly string name;

        public LoggerVariable(string name)
            : base(typeof(ILogger))
        {
            this.name = name;
        }

        public override string CtorArgDeclaration => $"{typeof(ILoggerFactory).FullNameInCode()} {ArgumentName}Factory";

        public override void WriteAssignment(ISourceWriter writer)
        {
            writer.WriteLine($"{Usage} = {ArgumentName}Factory.{nameof(ILoggerFactory.CreateLogger)}(\"{name}\");");
        }
    }
}
