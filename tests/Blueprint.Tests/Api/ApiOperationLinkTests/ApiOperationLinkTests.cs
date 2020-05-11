using Blueprint.Api;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ApiOperationLinkTests
{
    public class ApiOperationLinkTests
    {
        private class LinkGeneratorTestsOperation : IApiOperation
        {
            public int ClientId { get; set; }

            public string Category { get; set; }
        }

        [Test]
        public void When_Created_Then_Exposes_Trimmed_UrlFormat_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.UrlFormat.Should().Be("aUrl");
        }

        [Test]
        public void When_Created_Then_Exposes_Operation_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof (LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.OperationDescriptor.Should().Be(descriptor);
        }

        [Test]
        public void When_Created_Then_Exposes_Rel_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.Rel.Should().Be("a.rel");
        }

        [Test]
        public void When_Placeholder_Has_Alternate_Name_Then_RoutingUrl_Strips()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{ClientId:Id}", "a.rel");

            // Assert
            link.RoutingUrl.Should().Be("aUrl/{ClientId}");
        }

        [Test]
        public void When_Placeholder_Has_Alternate_Name_Then_Placeholder_Created_With_AlternatePropertyName()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{ClientId:Id}", "a.rel");

            // Assert
            link.Placeholders[0].AlternatePropertyName.Should().Be("Id");
            link.Placeholders[0].OriginalText.Should().Be("{ClientId:Id}");
            link.Placeholders[0].Property.Name.Should().Be("ClientId");
            link.Placeholders[0].Format.Should().BeNull();
        }

        [Test]
        public void When_Placeholder_Has_Different_Case_Then_Placeholder_Created_Correctly()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid}", "a.rel");

            // Assert
            link.Placeholders[0].OriginalText.Should().Be("{clientid}");
            link.Placeholders[0].Property.Name.Should().Be("ClientId");
            link.Placeholders[0].Format.Should().BeNull();
            link.Placeholders[0].AlternatePropertyName.Should().BeNull();
        }

        [Test]
        public void When_Placeholder_Has_Different_Case_RoutingUrl_Should_Normalise()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientId:Id}", "a.rel");

            // Assert
            link.RoutingUrl.Should().Be("aUrl/{ClientId}");
        }

        [Test]
        public void When_Format_Has_QueryString_RoutingUrl_Strips()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}?format=pdf", "a.rel");

            // Assert
            link.RoutingUrl.Should().Be("aUrl/{ClientId}");
        }
    }
}
