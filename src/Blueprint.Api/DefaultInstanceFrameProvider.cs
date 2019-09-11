using System;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;

namespace Blueprint.Api
{
    public class DefaultInstanceFrameProvider : IInstanceFrameProvider
    {
        public GetInstanceFrame<T> VariableFromContainer<T>(GeneratedType generatedType, Type toLoad)
        {
            return new TransientInstanceFrame<T>(toLoad);
        }
    }
}
