using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

#if !NET472
using System.Runtime.Loader;
#endif

namespace Blueprint.Compiler
{
    public class InMemoryOnlyCompileStrategy : ICompileStrategy
    {
        public Assembly Compile(ILogger logger, CSharpCompilation compilation, Action<EmitResult> check)
        {
            logger.LogInformation("Compiling source to an in-memory DLL with embedded source");

            using (var assemblyStream = new MemoryStream())
            {
                var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded);
                var embeddedTexts = compilation.SyntaxTrees.Select(s => EmbeddedText.FromSource(s.FilePath, s.GetText()));

                var result = compilation.Emit(
                    peStream: assemblyStream,
                    embeddedTexts: embeddedTexts,
                    options: emitOptions);

                check(result);

                assemblyStream.Seek(0, SeekOrigin.Begin);

#if !NET472
                return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
#else
                return Assembly.Load(assemblyStream.ToArray());
#endif
            }
        }
    }
}
