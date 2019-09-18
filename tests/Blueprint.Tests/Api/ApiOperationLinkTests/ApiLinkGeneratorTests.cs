using System;
using System.Net.Http;
using Blueprint.Api;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ApiOperationLinkTests
{
    public class ApiLinkGeneratorTests
    {
        private readonly ApiOperationDescriptor descriptor = new ApiOperationDescriptor(typeof(TestApiOperation), HttpMethod.Get);

        private ApiDataModel dataModel;
        private ApiConfiguration configuration;
        private ApiLinkGenerator linkGenerator;

        [SetUp]
        public void CreateGenerator()
        {
            dataModel = new ApiDataModel();
            configuration = new ApiConfiguration();
            linkGenerator = new ApiLinkGenerator(configuration, dataModel);
        }

        [Test]
        public void When_AbsoluteUrl_Prepends_Configuration_Base_Url()
        {
            // Arrange
            linkGenerator = new ApiLinkGenerator(new ApiConfiguration
            {
                BaseApiUrl = "http://api.example.com/api/"
            }, dataModel);

            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new object()).Should().Be("http://api.example.com/api/aUrl");
        }

        [Test]
        public void When_Url_No_Placeholders_CreateUrl_Returns_Url_As_Is()
        {
            // Arrange
            var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new object()).Should().EndWith("aUrl");
        }

        [Test]
        public void When_Url_With_Simple_Placeholder_Then_CreateUrl_Replaces_With_Property_In_PropValues()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new { category = "cats" }).Should().EndWith("aUrl/cats");
        }

        [Test]
        public void When_Url_With_Placeholder_Requring_Encoding_Then_CreateUrl_Replaces_With_Encoded_Property_In_PropValues()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new { category = "all dogs" }).Should().EndWith("aUrl/all%20dogs");
        }

        [Test]
        public void When_Placeholder_Specifies_Alternate_Property_Then_Uses_That_Property_From_Instance_Values()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new { id = 15484 }).Should().EndWith("aUrl/15484");
        }

        [Test]
        public void When_Placeholder_Specifies_Alternate_Property_And_Followed_By_Another_Placeholder_Then_Uses_That_Property_From_Instance_Values()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}/{aProperty}", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new { id = 15484, aProperty = "value" }).Should().EndWith("aUrl/15484/value");
        }

        [Test]
        public void When_Placeholder_Has_Format_Then_CreateUrl_Uses_Format()
        {
            // Act
            var date = new DateTime(2012, 04, 21);
            var link = new ApiOperationLink(descriptor, "/aUrl/{date(yyyy-MM-dd)}", "a.rel");

            // Assert
            linkGenerator.CreateUrlFromLink(link, new { date }).Should().EndWith("aUrl/2012-04-21");
        }

        [Test]
        public void When_Placeholder_Has_Separate_Source_Then_GetSafeRoutingUrl_Strips()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}", "a.rel");

            // Assert
            link.GetFormatForRouting().Should().Be("aUrl/{clientid}");
        }

        [Test]
        public void When_Format_Has_QueryString_UrlFormat_Strips()
        {
            // Act
            var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}?format=pdf", "a.rel");

            // Assert
            link.GetFormatForRouting().Should().Be("aUrl/{clientid}");
        }
    }
}
