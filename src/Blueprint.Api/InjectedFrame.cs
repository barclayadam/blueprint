using System.Collections.Generic;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;

namespace Blueprint.Api
{
    public class InjectedFrame<T> : GetInstanceFrame<T>
    {
        public InjectedFrame(InjectedField field)
        {
            InstanceVariable = field;
        }

        public override IEnumerable<Variable> Creates => new[] { InstanceVariable };

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            // DO nothing here, we need to have this class so we can return a GetInstanceFrame
            // instance, but the actual variable is injected and therefore we need no code output
            Next?.GenerateCode(method, writer);
        }
    }
}
