using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.OperationExecutorBuilders;

public class Given_ApiOperationHandler
{
    [Test]
    public async Task When_Handler_Exists_In_IoC_And_Implements_Multiple_Interfaces_Can_Find_For_All()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(
            o => o
                .WithOperation<OperationA>()
                .WithOperation<OperationB>(),
            s =>
            {
                s.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler>(_ => new ScanOperationHandler(operationAResult: "IoCAResult"));
                s.AddSingleton<IApiOperationHandler<OperationB>, ScanOperationHandler>(_ => new ScanOperationHandler(operationBResult: "IoCBResult"));
            });

        // Act
        var resultA = await executor.ExecuteWithNewScopeAsync(new OperationA());
        var resultB = await executor.ExecuteWithNewScopeAsync(new OperationB());

        // Assert
        var okResultA = resultA.ShouldBeOperationResultType<OkResult>();
        okResultA.Content.Should().Be("IoCAResult");

        var okResultB = resultB.ShouldBeOperationResultType<OkResult>();
        okResultB.Content.Should().Be("IoCBResult");
    }

    [Test]
    public async Task When_Handler_Exists_In_IoC_Finds()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(
            o => o.WithOperation<OperationA>(),
            s => s.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler>(_ => new ScanOperationHandler(operationAResult: "IoCAResult")));

        // Act
        var resultA = await executor.ExecuteWithNewScopeAsync(new OperationA());

        // Assert
        var okResultA = resultA.ShouldBeOperationResultType<OkResult>();
        okResultA.Content.Should().Be("IoCAResult");
    }

    [Test]
    public async Task When_Handler_Registered_As_Interface_Exists_In_IoC_Finds()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(
            o => o.WithOperation<OperationA>(),
            s => s.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler>(_ => new ScanOperationHandler(operationAResult: "IoCAResult")));

        // Act
        var resultA = await executor.ExecuteWithNewScopeAsync(new OperationA());

        // Assert
        var okResultA = resultA.ShouldBeOperationResultType<OkResult>();
        okResultA.Content.Should().Be("IoCAResult");
    }

    public class OperationA
    {
    }

    public class OperationB
    {
    }

    public class ScanOperationHandler : IApiOperationHandler<OperationA>, IApiOperationHandler<OperationB>
    {
        public const string OperationADefaultResult = "OperationA";
        public const string OperationBDefaultResult = "OperationB";

        [CanBeNull] private readonly string operationAResult;
        [CanBeNull] private readonly string operationBResult;

        public ScanOperationHandler(
            [CanBeNull] string operationAResult = OperationADefaultResult,
            [CanBeNull] string operationBResult = OperationBDefaultResult)
        {
            this.operationAResult = operationAResult;
            this.operationBResult = operationBResult;
        }

        public ValueTask<object> Handle(OperationA operation, ApiOperationContext apiOperationContext)
        {
            return new ValueTask<object>(this.operationAResult);
        }

        public ValueTask<object> Handle(OperationB operation, ApiOperationContext apiOperationContext)
        {
            return new ValueTask<object>(this.operationBResult);
        }
    }
}
