using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Builder
{
    public class Given_CancellationToken
    {
        [Test]
        public async Task When_CancellationToken_cancelled_UnhandledExceptionOperationResult_returned()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource();
            cancellationToken.Cancel();

            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<CancellableOperation>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new CancellableOperation(), cancellationToken.Token);

            // Assert
            var exceptionResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            exceptionResult.Exception.Should().BeOfType<OperationCanceledException>();
        }

        [Test]
        public async Task When_CancellationToken_not_cancelled_runs_to_completion()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource();

            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<CancellableOperation>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new CancellableOperation(), cancellationToken.Token);

            // Assert
            result.Should().BeOfType<NoResultOperationResult>();
        }

        public class CancellableOperation : IApiOperation
        {
            public void InvokeAsync(CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
