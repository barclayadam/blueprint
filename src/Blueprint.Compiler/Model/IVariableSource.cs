using System;

namespace Blueprint.Compiler.Model
{
    public interface IVariableSource
    {
        Variable TryFindVariable(IMethodVariables variables, Type type);
    }
}
