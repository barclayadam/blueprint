using System;

namespace Blueprint.Compiler.Model
{
    public interface IVariableSource
    {
        Variable TryFindVariable(Type type);
    }
}
