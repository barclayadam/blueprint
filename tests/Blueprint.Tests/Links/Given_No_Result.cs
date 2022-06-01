using System.Threading.Tasks;
using Blueprint.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using VerifyNUnit;

namespace Blueprint.Tests.LinkGeneration
{
    public class Given_No_Result
    {
        // We need to ensure that if no OperationResult can be generated (i.e. in the case of multiple handlers) that
        // link middleware does not cause a compilation failure.
        [Test]
        public async Task When_No_Result_Generated_By_Handler_Then_Can_Compile()
        {
            // Arrange
            var baseHandler = new TestApiOperationHandler<OperationBase>("ignored");
            var child1Handler = new TestApiOperationHandler<OperationChild1>("ignored");
            var child2Handler = new TestApiOperationHandler<OperationChild2>("ignored");

            var executor = TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .WithHandler(baseHandler)
                    .WithHandler(child1Handler)
                    .WithHandler(child2Handler)
                    .WithOperation<OperationBase>(c => c.RequiresReturnValue = false)
                    .WithOperation<OperationChild1>(c => c.RequiresReturnValue = false)
                    .WithOperation<OperationChild2>(c => c.RequiresReturnValue = false));

            // Act
            var result = executor.WhatCodeDidIGenerateFor<OperationChild2>();

            // Assert
            await Verifier.Verify(result);
        }

        public class OperationBase {}
        
        public class OperationChild1 : OperationBase {}
        
        public class OperationChild2 : OperationBase {}
    }
}
