using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Compiler
{
    public class GeneratedAssembly
    {
        private readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();

        public GeneratedAssembly(GenerationRules generationRules)
        {
            GenerationRules = generationRules;
        }

        public GenerationRules GenerationRules { get; }

        public readonly List<GeneratedType> GeneratedTypes = new List<GeneratedType>();

        public void ReferenceAssembly(Assembly assembly)
        {
            assemblies.Add(assembly);
        }
        
        public GeneratedType AddType(string typeName, Type baseType)
        {
            // TODO -- assert that it's been generated already?

            var generatedType = new GeneratedType(GenerationRules, typeName);
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

        public void CompileAll()
        {
            var generator = BuildGenerator(GenerationRules);

            foreach (var generatedType in GeneratedTypes)
            {
                foreach (var x in generatedType.AssemblyReferences())
                {
                    generator.ReferenceAssembly(x);
                }

                generatedType.ArrangeFrames();

                var namespaces = generatedType
                    .AllInjectedFields
                    .Select(x => x.ArgType.Namespace)
                    .Concat(new[]{typeof(Task).Namespace})
                    .Distinct()
                    .ToList();

                var writer = new SourceWriter();

                foreach (var ns in namespaces.OrderBy(x => x))
                {
                    writer.Write($"using {ns};");
                }

                writer.BlankLine();

                writer.Namespace(GenerationRules.ApplicationNamespace);

                generatedType.Write(writer);

                writer.FinishBlock();

                var code = writer.Code();

                generatedType.SourceCode = code;
                generator.AddFile(generatedType.TypeName + ".cs", code);
            }

            var assembly = generator.Generate();

            var generated = assembly.GetExportedTypes().ToArray();

            foreach (var generatedType in GeneratedTypes)
            {
                generatedType.FindType(generated);
            }
        }

        private AssemblyGenerator BuildGenerator(GenerationRules generation)
        {
            var generator = new AssemblyGenerator(generation);

            foreach (var assembly in this.assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }

            return generator;
        }
    }
}
