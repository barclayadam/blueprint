using System;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;

namespace Blueprint;

public class InjectedFrame<T> : GetInstanceFrame<T>
{
    public InjectedFrame(InjectedField field)
    {
        this.InstanceVariable = field;
    }

    protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
    {
        // Do nothing here, we need to have this class so we can return a GetInstanceFrame
        // instance, but the actual variable is injected and therefore we need no code output
        next();
    }
}