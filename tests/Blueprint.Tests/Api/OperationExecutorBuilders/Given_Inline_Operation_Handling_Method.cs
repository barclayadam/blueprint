using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.OperationExecutorBuilders
{
    public class Given_Inline_Operation_Handling_Method
    {
        [Test]
        public async Task When_Handle_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineHandle>();
        }

        [Test]
        public async Task When_HandleAsync_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineHandleAsync>();
        }

        [Test]
        public async Task When_Invoke_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineInvoke>();
        }

        [Test]
        public async Task When_InvokeAsync_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineInvokeAsync>();
        }

        [Test]
        public async Task When_Execute_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineExecute>();
        }

        [Test]
        public async Task When_ExecuteAsync_Then_Executed()
        {
            await ShouldCallInlineMethod<InlineExecuteAsync>();
        }

        private async Task ShouldCallInlineMethod<T>() where T : IApiOperation, new()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<T>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new T());

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(typeof(T).Name);
        }

        public class InlineHandle : IApiOperation
        {
            public OkResult Handle()
            {
                return new OkResult(nameof(InlineHandle));
            }
        }

        public class InlineHandleAsync : IApiOperation
        {
            public Task<OkResult> HandleAsync()
            {
                return Task.FromResult(new OkResult(nameof(InlineHandleAsync)));
            }
        }

        public class InlineInvoke : IApiOperation
        {
            public OkResult Invoke()
            {
                return new OkResult(nameof(InlineInvoke));
            }
        }

        public class InlineInvokeAsync : IApiOperation
        {
            public Task<OkResult> InvokeAsync()
            {
                return Task.FromResult(new OkResult(nameof(InlineInvokeAsync)));
            }
        }

        public class InlineExecute : IApiOperation
        {
            public OkResult Execute()
            {
                return new OkResult(nameof(InlineExecute));
            }
        }

        public class InlineExecuteAsync : IApiOperation
        {
            public Task<OkResult> ExecuteAsync()
            {
                return Task.FromResult(new OkResult(nameof(InlineExecuteAsync)));
            }
        }
    }
}
