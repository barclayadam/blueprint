using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Validator_Middleware
{
    public class Given_ValidationMiddleware
    {
        public class HasRequiredPropertyOperation : IApiOperation
        {
            [Required]
            public object TheProperty { get; set; }
        }

        [Test]
        public async Task When_Empty_Operation_Then_Result_Executed()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddValidation()));

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new EmptyOperation());

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_Operation_Passes_Validation_Then_Result_Executed()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<HasRequiredPropertyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddValidation()));

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new HasRequiredPropertyOperation { TheProperty = "something not null"});

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_Operation_Does_Not_Pass_Validation_Then_Handler_Not_Executed()
        {
            // Arrange
            var handler = new TestApiOperationHandler<HasRequiredPropertyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddValidation()));

            // Act
            await executor.ExecuteWithNewScopeAsync(new HasRequiredPropertyOperation { TheProperty = null });

            // Assert
            handler.WasCalled.Should().BeFalse();
        }

        [Test]
        public async Task When_Operation_Does_Not_Pass_Validation_Then_ValidationResult_Returned()
        {
            // Arrange
            var handler = new TestApiOperationHandler<HasRequiredPropertyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddValidation()));

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new HasRequiredPropertyOperation { TheProperty = null });

            // Assert
            var validationResult = result.Should().BeOfType<ValidationFailedOperationResult>().Subject;
            validationResult.Errors.Should().ContainKey(nameof(HasRequiredPropertyOperation.TheProperty));
        }
    }
}
