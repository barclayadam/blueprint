﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Compiler
{
    /// <summary>
    /// A container of <see cref="GeneratedType" />s that can be collected together and compiled at runtime
    /// to generate a new <see cref="Assembly" /> from runtime generated code.
    /// </summary>
    public class GeneratedAssembly
    {
        private readonly GenerationRules _generationRules;
        private readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        private readonly List<GeneratedType> _generatedTypes = new List<GeneratedType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedAssembly"/> class.
        /// </summary>
        /// <param name="generationRules">A set of rules that determine <em>how</em> an assembly is generated / compiled.</param>
        public GeneratedAssembly(GenerationRules generationRules)
        {
            this._generationRules = generationRules;
        }

        /// <summary>
        /// A list of all <see cref="GeneratedTypes" /> that have been added to this assembly.
        /// </summary>
        public IReadOnlyList<GeneratedType> GeneratedTypes => this._generatedTypes;

        /// <summary>
        /// References the given assembly in the assembly when it is compiled.
        /// </summary>
        /// <param name="assembly">The assembly to reference.</param>
        public void ReferenceAssembly(Assembly assembly)
        {
            this._assemblies.Add(assembly);
        }

        /// <summary>
        /// Creates a new <see cref="GeneratedType" /> and adds it to this assembly.
        /// </summary>
        /// <param name="namespace">The namespace of the type.</param>
        /// <param name="typeName">The name of the type / class.</param>
        /// <param name="baseType">The base type of the new type, if any.</param>
        /// <returns>A new <see cref="GeneratedType" />.</returns>
        /// <exception cref="ArgumentException">If a type already exists.</exception>
        public GeneratedType AddType(string @namespace, string typeName, Type baseType)
        {
            if (this.GeneratedTypes.Any(t => t.Namespace == @namespace && t.TypeName == typeName))
            {
                throw new ArgumentException($"A type already exists at {@namespace}.{typeName}");
            }

            var generatedType = new GeneratedType(this, typeName, @namespace ?? this._generationRules.AssemblyName);

            if (baseType.IsInterface)
            {
                generatedType.Implements(baseType);
            }
            else
            {
                generatedType.InheritsFrom(baseType);
            }

            this._generatedTypes.Add(generatedType);

            return generatedType;
        }

        public void CompileAll(ICompileStrategy compileStrategy)
        {
            var generator = new AssemblyGenerator(compileStrategy);

            foreach (var assemblyReference in this._assemblies)
            {
                generator.ReferenceAssembly(assemblyReference);
            }

            foreach (var generatedType in this.GeneratedTypes)
            {
                foreach (var x in generatedType.AssemblyReferences())
                {
                    generator.ReferenceAssembly(x);
                }

                // We generate the code for the type upfront as we allow adding namespaces etc. during the rendering of
                // frames so we need to do those, and _then_ gather namespaces
                // A rough estimate of 3000 characters per method with 2 being used, plus 1000 for ctor.
                var typeWriter = new SourceWriter((3000 * 2) + 1000);
                generatedType.Write(typeWriter);

                var namespaces = generatedType
                    .AllInjectedFields
                    .Select(x => x.VariableType.Namespace)
                    .Concat(new[] { typeof(Task).Namespace })
                    .Concat(generatedType.UsingNamespaces)
                    .Distinct()
                    .ToList();

                var writer = new SourceWriter();

                writer.Comment("<auto-generated />");
                writer.Comment(generatedType.TypeName);
                writer.BlankLine();

                foreach (var ns in namespaces.OrderBy(x => x))
                {
                    writer.UsingNamespace(ns);
                }

                writer.BlankLine();

                writer.Namespace(generatedType.Namespace);

                writer.WriteLines(typeWriter.Code());

                writer.FinishBlock();

                var code = writer.Code();

                generatedType.GeneratedSourceCode = code;
                generator.AddFile($"{generatedType.Namespace.Replace(".", "/")}/{generatedType.TypeName}.cs", code);
            }

            var assembly = generator.Generate(this._generationRules);

            var generated = assembly.GetExportedTypes().ToArray();

            foreach (var generatedType in this.GeneratedTypes)
            {
                generatedType.FindType(generated);
            }
        }
    }
}
