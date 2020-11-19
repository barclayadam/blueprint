using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.CodeGen
{
    public class LoggerVariable : InjectedField
    {
        private readonly string _name;

        public LoggerVariable(string name)
            : base(typeof(ILogger))
        {
            this._name = name;
        }

        public override string CtorArgDeclaration => $"{typeof(ILoggerFactory).FullNameInCode()} {this.ArgumentName}Factory";

        public override void WriteAssignment(ISourceWriter writer)
        {
            writer.WriteLine($"{this.Usage} = {this.ArgumentName}Factory.{nameof(ILoggerFactory.CreateLogger)}(\"{this._name}\");");
        }
    }
}
