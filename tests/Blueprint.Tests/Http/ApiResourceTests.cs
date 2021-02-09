using System;
using Blueprint.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Http
{
    public class ApiResourceTests
    {
        [Test]
        public void When_adding_Link_with_duplicate_rel_throws_InvalidOperationException()
        {
            // Arrange
            var resource = new TestApiResource();

            resource.AddLink("rel1", new Link { Href = "https://rel1link.coim" });

            // Act
            Action tryAddDuplicate = () => resource.AddLink("rel1", new Link { Href = "https://anotherrel2link.com" });

            // Assert
            tryAddDuplicate.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage("A link with the rel 'rel1' (link href is https://anotherrel2link.com) has already been added to the API resource of type 'Blueprint.Tests.Http.ApiResourceTests+TestApiResource'. Existing link has href of https://rel1link.coim.\n\n" +
"Ensure that when adding links (see LinkAttribute, SelfLinkAttribute or RootLinkAttribute) that no two exist for the same resource type (including the root) with the same rel value.");
        }

        private class TestApiResource : ApiResource {}
    }
}
