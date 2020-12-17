using Blueprint.Configuration;
using Blueprint.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Blueprint.Tests.ApiOperationLinkTests
{
    public class SelfLinkTests
    {
        public class LinkGeneratorResource : ApiResource
        {
            public string Id { get; set; }
        }

        [SelfLink(typeof(LinkGeneratorResource), "/linkGenerators/{id}")]
        private class SelfLinkGeneratorTestsOperation
        {
            public string Id { get; set; }

            public string AnotherProperty { get; set; }
        }

        private BlueprintApiOptions options;
        private ApiLinkGenerator linkGenerator;

        [SetUp]
        public void CreateGenerator()
        {
            options = new BlueprintApiOptions();

            new OperationScanner()
                .AddOperation<SelfLinkGeneratorTestsOperation>()
                .FindOperations(options.Model);

            var httpContext = new DefaultHttpContext();
            httpContext.SetBaseUri("http://api.example.com/api/");

            linkGenerator = new ApiLinkGenerator(options.Model, new HttpContextAccessor { HttpContext = httpContext });
        }

        [Test]
        public void When_AbsoluteUrl_Prepends_Configuration_Base_Url()
        {
            // Arrange
            var link = new SelfLinkGeneratorTestsOperation
            {
                Id = "myId"
            };

            // Assert
            var selfLink = linkGenerator.CreateSelfLink<LinkGeneratorResource>(link);
            selfLink.Type.Should().Be("linkGenerator");
            selfLink.Href.Should().Be("http://api.example.com/api/linkGenerators/myId");
        }

        [Test]
        public void When_Extra_Properties_Then_Does_NOT_Append_As_QueryString()
        {
            // Arrange
            var link = new SelfLinkGeneratorTestsOperation
            {
                Id = "myId",
                AnotherProperty = "some-other-value"
            };

            // Assert
            var selfLink = linkGenerator.CreateSelfLink<LinkGeneratorResource>(link);
            selfLink.Type.Should().Be("linkGenerator");
            selfLink.Href.Should().EndWith("/linkGenerators/myId");
        }

        [Test]
        public void When_Extra_Properties_As_Argument_Then_Appends_As_QueryString()
        {
            // Arrange
            var link = new SelfLinkGeneratorTestsOperation
            {
                Id = "myId",
            };

            // Assert
            var selfLink = linkGenerator.CreateSelfLink<LinkGeneratorResource>(link.Id, new { format = "pdf" });
            selfLink.Type.Should().Be("linkGenerator");
            selfLink.Href.Should().EndWith("/linkGenerators/myId?format=pdf");
        }
    }
}
