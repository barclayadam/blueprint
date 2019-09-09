using System;

namespace Blueprint.Compiler.Model
{
    public class StaticField : Variable
    {
        private readonly string initializer;

        public StaticField(Type argType, string initializer) : this(argType, DefaultArgName(argType), initializer)
        {
        }

        public StaticField(Type argType, string name, string initializer) : base(argType, "_" + name)
        {
            this.initializer = initializer;
        }

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.Write($"private static readonly {VariableType.FullNameInCode()} {Usage} = {initializer};");
        }
    }
}
