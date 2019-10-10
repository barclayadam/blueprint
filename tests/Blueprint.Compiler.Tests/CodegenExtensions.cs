using Microsoft.Extensions.Logging.Abstractions;

namespace Blueprint.Compiler.Tests
{
    public static class CodegenExtensions
    {
        public static void CompileAll(this GeneratedAssembly assembly)
        {
            assembly.CompileAll(new AssemblyGenerator(new NullLogger<AssemblyGenerator>()));
        }
    }
}
