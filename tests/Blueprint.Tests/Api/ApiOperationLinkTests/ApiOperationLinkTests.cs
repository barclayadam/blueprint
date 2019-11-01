using System.Net.Http;
using Blueprint.Api;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ApiOperationLinkTests
{
    public class ApiOperationLinkTests
    {
        [RootLink("/aUrl/{clientId}/{category}/some-more", Rel = "a.rel")]
        private class LinkGeneratorTestsOperation : IApiOperation
        {
            public int ClientId { get; set; }

            public string Category { get; set; }
        }

        [Test]
        public void When_Created_Then_Exposes_Trimmed_UrlFormat_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.UrlFormat.Should().Be("aUrl");
        }

        [Test]
        public void When_Created_Then_Exposes_Operation_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof (LinkGeneratorTestsOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.OperationDescriptor.Should().Be(descriptor);
        }

        [Test]
        public void When_Created_Then_Exposes_Rel_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.Rel.Should().Be("a.rel");
        }

        [Test]
        public void When_Placeholder_Has_Separate_Source_Then_GetSafeRoutingUrl_Strips()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}", "a.rel");

            // Assert
            link.RoutingUrl.Should().Be("aUrl/{clientid}");
        }

        [Test]
        public void When_Format_Has_QueryString_UrlFormat_Strips()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}?format=pdf", "a.rel");

            // Assert
            link.RoutingUrl.Should().Be("aUrl/{clientid}");
        }
    }
}
