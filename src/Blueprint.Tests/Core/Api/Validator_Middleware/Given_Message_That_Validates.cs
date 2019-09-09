using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Middleware;
using Blueprint.Core;

using NUnit.Framework;

using Shouldly;

using StructureMap;

namespace Blueprint.Tests.Core.Api.Validator_Middleware
{
    public class Given_ValidationMiddleware
    {
        public class EmptyOperation : IApiOperation
        {
        }

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

            // Act
            var result = await Execute(new EmptyOperation(), toReturn);

            // Assert
            result.Result.ShouldBeOfType<OkResult>();
            ((OkResult)result.Result).Content.ShouldBe(toReturn);
            result.Handler.WasCalled.ShouldBeTrue();
        }

        [Test]
        public async Task When_Operation_Passes_Validation_Then_Result_Executed()
        {
            // Arrange
            var toReturn = 12345;

            // Act
            var result = await Execute(new HasRequiredPropertyOperation { TheProperty = "something not null"}, toReturn);

            // Assert
            result.Result.ShouldBeOfType<OkResult>();
            ((OkResult)result.Result).Content.ShouldBe(toReturn);
            result.Handler.WasCalled.ShouldBeTrue();
        }

        [Test]
        public async Task When_Operation_Does_Not_Pass_Validation_Then_Handler_Not_Executed()
        {
            // Arrange
            var toReturn = 12345;

            // Act
            var result = await Execute(new HasRequiredPropertyOperation { TheProperty = null }, toReturn);

            // Assert
            result.Handler.WasCalled.ShouldBeFalse();
        }

        [Test]
        public async Task When_Operation_Does_Not_Pass_Validation_Then_ValidationResult_Returned()
        {
            // Arrange
            var toReturn = 12345;

            // Act
            var result = await Execute(new HasRequiredPropertyOperation { TheProperty = null }, toReturn);

            // Assert
            result.Result.ShouldBeOfType<ValidationFailedResult>();
            ((ValidationFailedResult)result.Result).Content.Errors.ShouldContainKey(nameof(HasRequiredPropertyOperation.TheProperty));
        }

        private async Task<(OperationResult Result, TestApiOperationHandler<T> Handler)> Execute<T>(
            T operation,
            object toReturn) where T : IApiOperation
        {
            var handler = new TestApiOperationHandler<T>(toReturn);

            var options = new BlueprintApiOptions(o =>
            {
                o.WithApplicationName("Blueprint.Tests");

                o.UseMiddlewareBuilder<ValidationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.AddOperation<T>();
            });

            var container = ConfigureContainer(handler);
            var executor = new ApiOperationExecutorBuilder().Build(options, container);
            var context = options.Model.CreateOperationContext<T>(container, operation);

            var result = await executor.Execute(context);

            return (result, handler);
        }

        private static Container ConfigureContainer<T>(IApiOperationHandler<T> handler) where T : IApiOperation
        {
            return new Container(c =>
            {
                c.AddRegistry<BlueprintRegistry>();
                c.For<IApiOperationHandler<T>>().Use(handler).Singleton();
            });
        }
    }
}
