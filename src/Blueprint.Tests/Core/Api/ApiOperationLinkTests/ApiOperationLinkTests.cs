using System.Net.Http;

using Blueprint.Core.Api;

using FluentAssertions;

using NUnit.Framework;

namespace Blueprint.Tests.Core.Api.ApiOperationLinkTests
{
    public class ApiOperationLinkTests
    {
        [Test]
        public void When_Created_Then_Exposes_Trimmed_UrlFormat_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(TestApiOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.UrlFormat.Should().Be("aUrl");
        }

        [Test]
        public void When_Created_Then_Exposes_Operation_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof (TestApiOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.OperationDescriptor.Should().Be(descriptor);
        }

        [Test]
        public void When_Created_Then_Exposes_Rel_Property()
        {
            // Arrange
            var descriptor = new ApiOperationDescriptor(typeof(TestApiOperation), HttpMethod.Get);

            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            link.Rel.Should().Be("a.rel");
        }
    }
}
