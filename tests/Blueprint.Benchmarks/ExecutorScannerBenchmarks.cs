using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Blueprint.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Benchmarks;

[MemoryDiagnoser]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class ExecutorScannerBenchmarks
{
    private static readonly ExecutorScanner executorScanner = new ExecutorScanner();
    private static readonly OperationScanner operationScanner = new OperationScanner();
    private static readonly ServiceCollection services = new ServiceCollection();
    private static readonly List<ApiOperationDescriptor> operations = new List<ApiOperationDescriptor>();

    [GlobalSetup]
    public void Setup()
    {
        operationScanner.AddOperation(typeof(OperationA));
        operationScanner.AddOperation(typeof(OperationB));
        operationScanner.AddOperation(typeof(OperationC));
        operationScanner.AddOperation(typeof(OperationD));
        operationScanner.AddOperation(typeof(OperationE));
        operationScanner.AddOperation(typeof(OperationF));
        operationScanner.AddOperation(typeof(OperationG));
        operationScanner.AddOperation(typeof(OperationH));
        operationScanner.AddOperation(typeof(OperationI));
        operationScanner.AddOperation(typeof(OperationJ));
        operationScanner.AddOperation(typeof(OperationK));
        operationScanner.AddOperation(typeof(OperationL));
        operationScanner.AddOperation(typeof(OperationM));
        operationScanner.AddOperation(typeof(OperationN));
        operationScanner.AddOperation(typeof(OperationO));
        operationScanner.AddOperation(typeof(OperationP));
        operationScanner.AddOperation(typeof(OperationQ));
        operationScanner.AddOperation(typeof(OperationR));
        operationScanner.AddOperation(typeof(OperationS));
        operationScanner.AddOperation(typeof(OperationT));
        operationScanner.AddOperation(typeof(OperationU));
        operationScanner.AddOperation(typeof(OperationV));
        operationScanner.AddOperation(typeof(OperationW));
        operationScanner.AddOperation(typeof(OperationX));
        operationScanner.AddOperation(typeof(OperationY));
        operationScanner.AddOperation(typeof(OperationZ));

        operationScanner.Scan(typeof(ExecutorScanner).Assembly);
        operationScanner.Scan(typeof(ExecutorScannerBenchmarks).Assembly);

        for (var i = 0; i < 500; i++)
        {
            services.Add(new ServiceDescriptor(typeof(IApiOperationHandler<string>), typeof(ScanOperationHandler<string>), ServiceLifetime.Scoped));
        }

        services.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler<OperationA>>();
        services.AddSingleton<IApiOperationHandler<OperationB>, ScanOperationHandler<OperationB>>();
        services.AddSingleton<IApiOperationHandler<OperationC>, ScanOperationHandler<OperationC>>();
        services.AddSingleton<IApiOperationHandler<OperationD>, ScanOperationHandler<OperationD>>();
        services.AddSingleton<IApiOperationHandler<OperationE>, ScanOperationHandler<OperationE>>();
        services.AddSingleton<IApiOperationHandler<OperationF>, ScanOperationHandler<OperationF>>();
        services.AddSingleton<IApiOperationHandler<OperationG>, ScanOperationHandler<OperationG>>();

        operations.Add(new ApiOperationDescriptor(typeof(OperationA), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationB), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationC), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationD), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationE), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationF), "Benchmark"));
        operations.Add(new ApiOperationDescriptor(typeof(OperationG), "Benchmark"));
    }

    [Benchmark(Baseline = true)]
    public void Base()
    {
        executorScanner.FindAndRegister(operationScanner, services, operations);
    }

    private class OperationA {}
    private class OperationB {}
    private class OperationC {}
    private class OperationD {}
    private class OperationE {}
    private class OperationF {}
    private class OperationG {}
    private class OperationH {}
    private class OperationI {}
    private class OperationJ {}
    private class OperationK {}
    private class OperationL {}
    private class OperationM {}
    private class OperationN {}
    private class OperationO {}
    private class OperationP {}
    private class OperationQ {}
    private class OperationR {}
    private class OperationS {}
    private class OperationT {}
    private class OperationU {}
    private class OperationV {}
    private class OperationW {}
    private class OperationX {}
    private class OperationY {}
    private class OperationZ {}

    private class ScanOperationHandler<T> : IApiOperationHandler<T>
    {
        public ValueTask<object> Handle(T operation, ApiOperationContext apiOperationContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
