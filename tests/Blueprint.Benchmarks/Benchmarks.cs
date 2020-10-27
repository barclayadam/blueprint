using BenchmarkDotNet.Attributes;
using Blueprint.Compiler;

namespace Blueprint.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private const string textToWrite = @"using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using Blueprint.Compiler;

namespace Blueprint.Benchmarks
{
    public class SourceWriterBenchmarks
    {
        [Benchmark]
        public void Scenario1()
        {
            var sourceWriter = new SourceWriter();
            
            sourceWriter.Write("""");
        }
    }
}";

        [Benchmark]
        public void Base()
        {
            var sourceWriter = new SourceWriter
            {
                IndentationLevel = 1,
            };

            sourceWriter.Write(textToWrite);
        }
    }
}
