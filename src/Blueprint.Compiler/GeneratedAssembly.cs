using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Compiler
{
    public class GeneratedAssembly
    {
        private readonly GenerationRules generationRules;
        private readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();

        public GeneratedAssembly(GenerationRules generationRules)
        {
            this.generationRules = generationRules;
        }

        public List<GeneratedType> GeneratedTypes { get; } = new List<GeneratedType>();

        public void ReferenceAssembly(Assembly assembly)
        {
            assemblies.Add(assembly);
        }

        public GeneratedType AddType(string typeName, Type baseType)
        {
            var generatedType = new GeneratedType(this, typeName);

            if (baseType.IsInterface)
            {
                generatedType.Implements(baseType);
            }
            else
            {
                generatedType.InheritsFrom(baseType);
            }

            GeneratedTypes.Add(generatedType);

            return generatedType;
        }

        public void CompileAll(AssemblyGenerator generator)
        {
            foreach (var assemblyReference in assemblies)
            {
                generator.ReferenceAssembly(assemblyReference);
            }

            foreach (var generatedType in GeneratedTypes)
            {
                foreach (var x in generatedType.AssemblyReferences())
                {
                    generator.ReferenceAssembly(x);
                }

                generatedType.ArrangeFrames();

                var namespaces = generatedType
                    .AllInjectedFields
                    .Select(x => x.VariableType.Namespace)
                    .Concat(new[] { typeof(Task).Namespace })
                    .Distinct()
                    .ToList();

                var writer = new SourceWriter();

                foreach (var ns in namespaces.OrderBy(x => x))
                {
                    writer.Write($"using {ns};");
                }

                writer.BlankLine();

                writer.Namespace(generationRules.ApplicationNamespace);

                generatedType.Write(writer);

                writer.FinishBlock();

                var code = writer.Code();

                generatedType.SourceCode = code;
                generator.AddFile(generatedType.TypeName + ".cs", code);
            }

            var assembly = generator.Generate(generationRules);

            var generated = assembly.GetExportedTypes().ToArray();

            foreach (var generatedType in GeneratedTypes)
            {
                generatedType.FindType(generated);
            }
        }
    }
}
