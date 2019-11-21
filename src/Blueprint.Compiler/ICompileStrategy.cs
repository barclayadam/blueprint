using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Blueprint.Compiler
{
    public interface ICompileStrategy
    {
        Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check);
    }
}
