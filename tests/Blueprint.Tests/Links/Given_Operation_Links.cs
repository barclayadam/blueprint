using System;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Links
{
    public class Given_Operation_Links
    {
        [Test]
        public void When_Duplicate_Self_Link_Different_ResourceType_Then_Success()
        {
            // Arrange
            var baseHandler = new TestApiOperationHandler<OperationWithDuplicateDifferentResourceType>("ignored");

            // Act
            var executor = TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .WithHandler(baseHandler));

            // Assert
            executor.DataModel
                .GetLinksForOperation(typeof(OperationWithDuplicateDifferentResourceType))
                .Should()
                .HaveCount(2);
        }
        
        [Test]
        public void When_Duplicate_Link_Then_Exception()
        {
            // Arrange
            var baseHandler = new TestApiOperationHandler<OperationWithDuplicate>("ignored");

            Action tryCreateExecutor = () => TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .WithHandler(baseHandler));

            // Assert
            tryCreateExecutor.Should().ThrowExactly<InvalidOperationException>()
                .WithMessage("Could not register Root#rel1 => OperationWithDuplicate from WithHandler(TestApiOperationHandler`1) as it conflicts with existing Link registrations:" +
                             "\n\n" +
                             " Root#rel1 => OperationWithDuplicate sourced from WithHandler(TestApiOperationHandler`1)");
        }
        
        [Test]
        public void When_Duplicate_Rel_And_Resource_Type_Different_Url_Then_Fails()
        {
            // Arrange
            var baseHandler = new TestApiOperationHandler<OperationWithDuplicateResourceAndRel>("ignored");

            Action tryCreateExecutor = () => TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .WithHandler(baseHandler));

            // Assert
            tryCreateExecutor.Should().ThrowExactly<InvalidOperationException>()
                .WithMessage("Duplicate link found for resource type ApiResource1 with rel rel1. Existing link is for the operation: " +
                             "\n\n" +
                             " Blueprint.Tests.Links.Given_Operation_Links+OperationWithDuplicateResourceAndRel: the-same-link/" +
                             "\n\n" +
                             "Every link must be a unique pairing of resource type (i.e. UserApiResource) and rel (i.e. \"self\" or \"activate\")");
        }

        [Link(typeof(ApiResource1), "/the-same-link/", Rel = "rel1")]
        [Link(typeof(ApiResource1), "/the-same-link/", Rel = "rel1")]
        public class OperationWithDuplicate {}
        
        [Link(typeof(ApiResource1), "/the-same-link/", Rel = "rel1")]
        [Link(typeof(ApiResource1), "/a-different-link/", Rel = "rel1")]
        public class OperationWithDuplicateResourceAndRel {}
        
        [Link(typeof(ApiResource1), "/the-same-link/", Rel = "self")]
        [Link(typeof(ApiResource2), "/the-same-link/", Rel = "self")]
        public class OperationWithDuplicateDifferentResourceType {}
        
        public class ApiResource1 : ApiResource {}
        
        public class ApiResource2 : ApiResource {}
    }
}
