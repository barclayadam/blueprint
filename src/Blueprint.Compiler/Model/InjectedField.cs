using System;

namespace Blueprint.Compiler.Model
{
    public class InjectedField : Variable
    {
        public InjectedField(Type argType) : this(argType, DefaultArgName(argType))
        {
        }

        public InjectedField(Type argType, string name) : base(argType, "_" + name)
        {
            ArgumentName = name;
        }

        public string ArgumentName { get; }

        public virtual string CtorArgDeclaration => $"{VariableType.FullNameInCode()} {ArgumentName}";

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.Write($"private readonly {VariableType.FullNameInCode()} {Usage};");
        }

        public virtual void WriteAssignment(ISourceWriter writer)
        {
            writer.Write($"{Usage} = {ArgumentName};");
        }
    }
}
