using System;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;

namespace Blueprint.Api
{
    public interface IInstanceFrameProvider
    {
        GetInstanceFrame<T> VariableFromContainer<T>(GeneratedType generatedType, Type toLoad);
    }
}
