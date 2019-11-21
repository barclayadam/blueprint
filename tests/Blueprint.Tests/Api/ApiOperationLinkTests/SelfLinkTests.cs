using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ApiOperationLinkTests
{
    public class SelfLinkTests
    {
        public class LinkGeneratorResource : ApiResource
        {
            public string Id { get; set; }
        }

        [SelfLink(typeof(LinkGeneratorResource), "/linkGenerators/{id}")]
        private class SelfLinkGeneratorTestsOperation : IApiOperation
        {
            public string Id { get; set; }

            public string AnotherProperty { get; set; }
        }

        private ApiOperationDescriptor descriptor;
        private BlueprintApiOptions options;
        private ApiLinkGenerator linkGenerator;

        [SetUp]
        public void CreateGenerator()
        {
            options = new BlueprintApiOptions();
            options.AddOperation<SelfLinkGeneratorTestsOperation>();

            linkGenerator = new ApiLinkGenerator(options);
            descriptor = options.Model.Operations.Single(o => o.OperationType == typeof(SelfLinkGeneratorTestsOperation));
        }

        [Test]
        public void When_AbsoluteUrl_Prepends_Configuration_Base_Url()
        {
            // Arrange
            options.BaseApiUrl = "http://api.example.com/api/";
            linkGenerator = new ApiLinkGenerator(options);

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
            selfLink.Href.Should().Be("/linkGenerators/myId");
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
            selfLink.Href.Should().Be("/linkGenerators/myId?format=pdf");
        }
    }
}
