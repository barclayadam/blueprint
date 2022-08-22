using System;
using Blueprint.Http;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Blueprint.Tests.ApiOperationLinkTests;

public class ApiOperationLinkTests
{
    private class LinkGeneratorTestsOperation
    {
        public int ClientId { get; set; }

        [CanBeNull]
        public string Category { get; set; }

        public DateTime? Date { get; set; }

        [CanBeNull]
        public string SortBy { get; set; }

        public int? Page { get; set; }
    }
        
    private class LinkGeneratorTestsResource : ApiResource
    {
        public int Id { get; set; }
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
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");

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


    [Test]
    public void When_Url_No_Placeholders_CreateUrl_Returns_Url_As_Is()
    {
        // Arrange
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl", "a.rel");

        // Assert
        link.CreateRelativeUrl(new object()).Should().EndWith("aUrl");
    }

    [Test]
    public void When_Url_With_Simple_Placeholder_Then_CreateUrl_Replaces_With_Property_In_PropValues()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { category = "cats" }).Should().EndWith("aUrl/cats");
    }

    [Test]
    public void When_Url_With_Invalid_Placeholder_Then_Exception_thrown()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        Action tryCreate = () => new ApiOperationLink(descriptor, "/aUrl/{cannotBeFound}", "a.rel");

        // Assert
        tryCreate.Should().ThrowExactly<OperationLinkFormatException>()
            .WithMessage("URL /aUrl/{cannotBeFound} is invalid. Property cannotBeFound does not exist on operation type LinkGeneratorTestsOperation");
    }
        
    [Test]
    public void When_ResourceType_is_not_ILinkableResource_then_exception()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        Action tryCreate = () => new ApiOperationLink(descriptor, "/aUrl/{clientId:doesNotExist}", "a.rel", typeof(string));

        // Assert
        tryCreate.Should().ThrowExactly<OperationLinkFormatException>()
            .WithMessage("Resource type string is not assignable to ILinkableResource, cannot add a link for LinkGeneratorTestsOperation");
    }
        
    [Test]
    public void When_Url_With_Placeholder_With_Missing_Property_In_ResourceType_Then_Exception_Thrown()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        Action tryCreate = () => new ApiOperationLink(descriptor, "/aUrl/{clientId}", "a.rel", typeof(LinkGeneratorTestsResource));

        // Assert
        tryCreate.Should().ThrowExactly<OperationLinkFormatException>()
            .WithMessage("Link /aUrl/{clientId} for operation LinkGeneratorTestsOperation specifies placeholder ClientId that cannot be found on resource LinkGeneratorTestsResource");
    }
        
    [Test]
    public void When_Url_With_Invalid_Alternate_Placeholder_Then_Exception_Thrown()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        Action tryCreate = () => new ApiOperationLink(descriptor, "/aUrl/{clientId:doesNotExist}", "a.rel", typeof(LinkGeneratorTestsResource));

        // Assert
        tryCreate.Should().ThrowExactly<OperationLinkFormatException>()
            .WithMessage("Link /aUrl/{clientId:doesNotExist} for operation LinkGeneratorTestsOperation specifies placeholder {clientId:doesNotExist}. Cannot find alternate property doesNotExist on resource LinkGeneratorTestsResource");
    }

    [Test]
    public void When_Url_With_Placeholder_Requiring_Encoding_Then_CreateUrl_Replaces_With_Encoded_Property_In_PropValues()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { category = "all dogs" }).Should().EndWith("aUrl/all%20dogs");
    }

    [Test]
    public void When_Url_With_Placeholder_Then_Returns_Null_If_Property_Is_Null()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{category}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { category = (string)null }).Should().BeNull();
    }

    [Test]
    public void When_Placeholder_Specifies_Alternate_Property_Then_Uses_That_Property_From_Instance_Values()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { id = 15484 }).Should().EndWith("aUrl/15484");
    }

    [Test]
    public void When_Placeholder_Followed_By_Static_Then_Includes_All()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}/some-more", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { id = 15484 }).Should().EndWith("aUrl/15484/some-more");
    }

    [Test]
    public void When_Placeholder_Specifies_Alternate_Property_And_Followed_By_Another_Placeholder_Then_Uses_That_Property_From_Instance_Values()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/aUrl/{clientid:id}/{category}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { id = 15484, category = "value" }).Should().EndWith("aUrl/15484/value");
    }

    [Test]
    public void When_Placeholder_Has_Format_Then_CreateUrl_Uses_Format()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var date = new DateTime(2012, 04, 21);
        var link = new ApiOperationLink(descriptor, "/aUrl/{date(yyyy-MM-dd)}", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { date }).Should().EndWith("aUrl/2012-04-21");
    }

    [Test]
    public void When_Additional_Properties_And_Placeholders_Exists_Adds_As_QueryString_When_IncludeExtra_True()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/clients/{clientId}/users", "a.rel");

        // Act
        var url = link.CreateRelativeUrl(new LinkGeneratorTestsOperation { ClientId = 12, SortBy = "name", Page = 2 }, true);

        // Assert
        url.Should().Be("clients/12/users?SortBy=name&Page=2");
    }

    [Test]
    public void When_Additional_Properties_Exists_Adds_As_QueryString_When_IncludeExtra_True()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/users", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { sortBy = "name", page = 2 }, true).Should().Be("users?sortBy=name&page=2");
    }

    [Test]
    public void When_Additional_Properties_Exists_Ignore_When_IncludeExtra_False()
    {
        // Act
        var descriptor = new ApiOperationDescriptor(typeof(LinkGeneratorTestsOperation), "tests");
        var link = new ApiOperationLink(descriptor, "/users", "a.rel");

        // Assert
        link.CreateRelativeUrl(new { sortBy = "name", page = 2 }, false).Should().Be("users");
    }
}