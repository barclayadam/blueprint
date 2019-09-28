using System;

namespace Blueprint.Compiler.Model
{
    public interface IVariableSource
    {
        bool Matches(Type type);

        Variable Create(Type type);
    }
}
