using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Blueprint.Benchmarks
{
    [MemoryDiagnoser]
    public class CompileBenchmarks
    {
        private AssemblyGenerator generator;
        private GenerationRules generationRules;

        [GlobalSetup]
        public void SetupGenerator()
        {
            generator = new AssemblyGenerator(new DoNothingCompileStrategy());

            for (var i = 0; i < 50; i++)
            {
                generator.AddFile($"Benchmark{i}.cs", @"
namespace Blueprint.Benchmarks
{
    public class CompileBenchmarks{i}
    {
        public void Scenario1()
        {
            System.Console.WriteLine(""Hey!!"");
        }
    }
}".Replace("{i}", i.ToString()));
            }

            generationRules = new GenerationRules {AssemblyName = "Blueprint.Benchmarks"};
        }

        [Benchmark]
        public Assembly Base()
        {
            return generator.Generate(generationRules);
        }

        private class DoNothingCompileStrategy : ICompileStrategy
        {
            public Assembly TryLoadExisting(string sourceTextHash, string assemblyName)
            {
                return null;
            }

            public Assembly Compile(string sourceTextHash, CSharpCompilation compilation, Action<EmitResult> check)
            {
                return null;
            }
        }
    }
}
