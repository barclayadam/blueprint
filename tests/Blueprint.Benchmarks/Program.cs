using BenchmarkDotNet.Running;

namespace Blueprint.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ExecutorScannerBenchmarks>();
    }
}
