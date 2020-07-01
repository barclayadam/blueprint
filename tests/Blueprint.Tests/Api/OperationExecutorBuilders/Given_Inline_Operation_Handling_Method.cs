using System;
using System.Threading.Tasks;
using Blueprint.Http;
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

        [Test]
        public async Task When_Return_Is_Specific_Type_And_Async_Then_Executed()
        {
            await ShouldCallInlineMethod<SpecificReturnNonAsync>(okContent => okContent.Should().BeOfType<AnApiResource>());
        }

        [Test]
        public async Task When_Return_Is_Specific_Type_And_Non_Async_Then_Executed()
        {
            await ShouldCallInlineMethod<SpecificReturnAsync>(okContent => okContent.Should().BeOfType<AnApiResource>());
        }

        [Test]
        public async Task When_Returned_Object_Is_More_Specific_And_OperationResult_Does_Not_Convert()
        {
            // Arrange
            var okResult = new OkResult("theReturn");
            var operation = new RuntimeSpecificReturnAsync(okResult);
            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<RuntimeSpecificReturnAsync>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(operation);

            // Assert
            var actualOkResult = result.ShouldBeOperationResultType<OkResult>();
            actualOkResult.Should().BeSameAs(okResult);
        }

        private Task ShouldCallInlineMethod<T>() where T : IApiOperation, new()
        {
            return ShouldCallInlineMethod<T>(okContent => okContent.Should().Be(typeof(T).Name));
        }

        private async Task ShouldCallInlineMethod<T>(Action<object> assertContent) where T : IApiOperation, new()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<T>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new T());

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            assertContent(okResult.Content);
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

        public class SpecificReturnNonAsync : IApiOperation
        {
            public AnApiResource Execute()
            {
                return new AnApiResource();
            }
        }

        public class SpecificReturnAsync : IApiOperation
        {
            public Task<AnApiResource> ExecuteAsync()
            {
                return Task.FromResult(new AnApiResource());
            }
        }

        public class RuntimeSpecificReturnAsync : IApiOperation
        {
            private readonly object toReturn;

            public RuntimeSpecificReturnAsync(object toReturn)
            {
                this.toReturn = toReturn;
            }

            public Task<object> ExecuteAsync()
            {
                return Task.FromResult(toReturn);
            }
        }

        public class AnApiResource : ApiResource
        {
        }
    }
}
