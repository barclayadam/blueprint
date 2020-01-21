using System;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;

namespace Blueprint.Api
{
    public interface IInstanceFrameProvider
    {
        GetInstanceFrame<T> TryGetVariableFromContainer<T>(GeneratedType generatedType, Type toLoad);
    }
}
