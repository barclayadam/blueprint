using System;
using System.Linq;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Blueprint.Tests.ApiOperationLinkTests
{
    public class ApiLinkGeneratorTests
    {
        [RootLink("/aUrl/{clientId}/{category}/some-more")]
        private class LinkGeneratorTestsOperation
        {
            public int ClientId { get; set; }

            public string Category { get; set; }

            public string AnotherProp { get; set; }

            public int AndAnotherOne { get; set; }

            public DateTime Date { get; set; }
        }

        private ApiOperationDescriptor descriptor;
        private BlueprintApiOptions options;
        private ApiLinkGenerator linkGenerator;

        [SetUp]
        public void CreateGenerator()
        {
            options = new BlueprintApiOptions();

            new OperationScanner()
                .AddOperation<LinkGeneratorTestsOperation>()
                .FindOperations(options.Model);

            var httpContext = new DefaultHttpContext();
            httpContext.SetRequestUri("http://api.example.com/api/");
            httpContext.SetBaseUri("api/");

            linkGenerator = new ApiLinkGenerator(options.Model, new HttpContextAccessor { HttpContext = httpContext });
            descriptor = options.Model.FindOperation(typeof(LinkGeneratorTestsOperation));
        }

        public class CreateUrlFromLink : ApiLinkGeneratorTests
        {
            [Test]
            public void When_AbsoluteUrl_Prepends_Configuration_Base_Url()
            {
                // Arrange
                var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new object()).Should().Be("http://api.example.com/api/aUrl");
            }

            [Test]
            public void When_Url_No_Placeholders_CreateUrl_Returns_Url_As_Is()
            {
                // Arrange
                var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new object()).Should().EndWith("aUrl");
            }

            [Test]
            public void When_Url_With_Simple_Placeholder_Then_CreateUrl_Replaces_With_Property_In_PropValues()
            {
                // Act
                var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {category = "cats"}).Should().EndWith("aUrl/cats");
            }

            [Test]
            public void When_Url_With_Placeholder_Requring_Encoding_Then_CreateUrl_Replaces_With_Encoded_Property_In_PropValues()
            {
                // Act
                var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {category = "all dogs"}).Should().EndWith("aUrl/all%20dogs");
            }

            [Test]
            public void When_Placeholder_Specifies_Alternate_Property_Then_Uses_That_Property_From_Instance_Values()
            {
                // Act
                var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {id = 15484}).Should().EndWith("aUrl/15484");
            }

            [Test]
            public void When_Placeholder_Followed_By_Static_Then_Includes_All()
            {
                // Act
                var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}/some-more", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {id = 15484}).Should().EndWith("aUrl/15484/some-more");
            }

            [Test]
            public void When_Placeholder_Specifies_Alternate_Property_And_Followed_By_Another_Placeholder_Then_Uses_That_Property_From_Instance_Values()
            {
                // Act
                var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}/{category}", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {id = 15484, category = "value"}).Should().EndWith("aUrl/15484/value");
            }

            [Test]
            public void When_Placeholder_Has_Format_Then_CreateUrl_Uses_Format()
            {
                // Act
                var date = new DateTime(2012, 04, 21);
                var link = new ApiOperationLink(descriptor, "/aUrl/{date(yyyy-MM-dd)}", "a.rel");

                // Assert
                linkGenerator.CreateUrl(link, new {date}).Should().EndWith("aUrl/2012-04-21");
            }
        }

        public class From_Operation : ApiLinkGeneratorTests
        {
            [Test]
            public void When_AbsoluteUrl_Prepends_Configuration_Base_Url()
            {
                // Arrange
                var link = new LinkGeneratorTestsOperation
                {
                    Category = "the-category",
                    ClientId = 726
                };

                // Assert
                linkGenerator.CreateUrl(link).Should().Be("http://api.example.com/api/aUrl/726/the-category/some-more");
            }

            [Test]
            public void When_Extra_Properties_Then_Appends_As_QueryString()
            {
                // Arrange
                var link = new LinkGeneratorTestsOperation
                {
                    Category = "the-category",
                    ClientId = 726,
                    AnotherProp = "some value to escape",
                    AndAnotherOne = 1548
                };

                // Assert
                linkGenerator.CreateUrl(link).Should().EndWith("/aUrl/726/the-category/some-more?AnotherProp=some%20value%20to%20escape&AndAnotherOne=1548");
            }
        }
    }
}
