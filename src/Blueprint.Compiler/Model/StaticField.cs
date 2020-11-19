using System;

namespace Blueprint.Compiler.Model
{
    public class StaticField : Variable
    {
        private readonly string _initializer;

        public StaticField(Type argType, string initializer)
            : this(argType, DefaultArgName(argType), initializer)
        {
        }

        public StaticField(Type argType, string name, string initializer)
            : base(argType, "_" + name)
        {
            this._initializer = initializer;
        }

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine($"private static readonly {this.VariableType.FullNameInCode()} {this.Usage} = {this._initializer};");
        }
    }
}
