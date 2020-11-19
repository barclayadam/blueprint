using System;

namespace Blueprint.Compiler.Model
{
    public class InjectedField : Variable
    {
        public InjectedField(Type argType)
            : this(argType, DefaultArgName(argType))
        {
        }

        public InjectedField(Type argType, string name)
            : base(argType, "_" + name)
        {
            this.ArgumentName = name;
        }

        public string ArgumentName { get; }

        public virtual string CtorArgDeclaration => $"{this.VariableType.FullNameInCode()} {this.ArgumentName}";

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine($"private readonly {this.VariableType.FullNameInCode()} {this.Usage};");
        }

        public virtual void WriteAssignment(ISourceWriter writer)
        {
            writer.WriteLine($"{this.Usage} = {this.ArgumentName};");
        }


    }
}
