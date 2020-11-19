using BenchmarkDotNet.Attributes;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Benchmarks
{
    [MemoryDiagnoser]
    public class MethodCallBenchmarks
    {
        private GeneratedMethod generatedMethod;
        private SourceWriter sourceWriter;

        [IterationSetup]
        public void Setup()
        {
            var generatedAssembly = new GeneratedAssembly(new GenerationRules {AssemblyName = "Blueprint.Benchmarks.MethodCallBenchmarks"});
            var generatedType = generatedAssembly.AddType("Blueprint.Benchmarks", "MethodTester", typeof(object));

            generatedMethod = generatedType.AddVoidMethod("Test");
            sourceWriter = new SourceWriter
            {
                IndentationLevel = 1,
            };
        }

        [Benchmark]
        public string BaseWithNoArguments()
        {
            var methodCall = MethodCall.For<MethodCallBenchmarks>(b => b.AnExampleMethod());

            methodCall.Target = new Variable(typeof(MethodCallBenchmarks));

            generatedMethod.Frames.Add(methodCall);

            generatedMethod.WriteMethod(sourceWriter);

            return sourceWriter.ToString();
        }

        [Benchmark]
        public string BaseWithArguments()
        {
            var methodCall = MethodCall.For<MethodCallBenchmarks>(b => b.AnExampleMethodWithArguments(default, default, default));

            methodCall.Target = new Variable(typeof(MethodCallBenchmarks));
            methodCall.Arguments[0] = new Variable(typeof(int));
            methodCall.Arguments[1] = new Variable(typeof(string));
            methodCall.Arguments[2] = new Variable(typeof(object));

            generatedMethod.Frames.Add(methodCall);

            generatedMethod.WriteMethod(sourceWriter);

            return sourceWriter.ToString();
        }

        [Benchmark]
        public string BaseStatic()
        {
            var methodCall = new MethodCall(typeof(MethodCallBenchmarks), nameof(AStatic));

            generatedMethod.Frames.Add(methodCall);

            generatedMethod.WriteMethod(sourceWriter);

            return sourceWriter.ToString();
        }

        private void AnExampleMethod()
        {
        }

        private void AnExampleMethodWithArguments(int arg1, string arg2, object arg3)
        {
        }

        public static void AStatic()
        {
        }
    }
}
