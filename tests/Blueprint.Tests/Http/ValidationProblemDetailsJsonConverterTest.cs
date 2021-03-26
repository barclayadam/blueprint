// Originally from https://github.com/dotnet/aspnetcore/blob/602cb1dea59751a1297d674cddf7f0729c23e6ca/src/Mvc/Mvc.Core/test/Infrastructure/ValidationProblemDetailsJsonConverterTest.cs

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Blueprint.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Http
{
    public class ValidationProblemDetailsJsonConverterTest
    {
        private static JsonSerializerOptions JsonSerializerOptions => BlueprintJsonOptions.DefaultSerializerOptions;

        [Test]
        public void Read_Works()
        {
            // Arrange
            var type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            var title = "Not found";
            var status = 404;
            var detail = "Product not found";
            var instance = "http://example.com/products/14";
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"detail\":\"{detail}\", \"instance\":\"{instance}\",\"traceId\":\"{traceId}\"," +
                "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";
            var converter = new ValidationProblemDetailsJsonConverter();
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var problemDetails = converter.Read(ref reader, typeof(ValidationProblemDetails), JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Instance.Should().Be(instance);
            problemDetails.Detail.Should().Be(detail);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
            problemDetails.Errors["key0"].Should().BeEquivalentTo("error0");
            problemDetails.Errors["key1"].Should().BeEquivalentTo("error1", "error2");
        }

        [Test]
        public void Read_WithSomeMissingValues_Works()
        {
            // Arrange
            var type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            var title = "Not found";
            var status = 404;
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"," +
                "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";
            var converter = new ValidationProblemDetailsJsonConverter();
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var problemDetails = converter.Read(ref reader, typeof(ValidationProblemDetails), JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
            problemDetails.Errors["key0"].Should().BeEquivalentTo("error0");
            problemDetails.Errors["key1"].Should().BeEquivalentTo("error1", "error2");
        }

        [Test]
        public void ReadUsingJsonSerializerWorks()
        {
            // Arrange
            var type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            var title = "Not found";
            var status = 404;
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"," +
                "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";

            // Act
            var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
            problemDetails.Errors["key0"].Should().BeEquivalentTo("error0");
            problemDetails.Errors["key1"].Should().BeEquivalentTo("error1", "error2");
        }

        [Test]
        public void Write_Works()
        {
            // Arrange
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var value = new ValidationProblemDetails(new Dictionary<string, IEnumerable<string>>
            {
                { "Property0", new [] { "error0" } },
                { "Property1", new [] { "error1", "error2" } }
            })
            {
                Title = "Not found",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 404,
                Detail = "Product not found",
                Instance = "http://example.com/products/14",
                Extensions = new Dictionary<string, object>
                {
                    { "traceId", traceId },
                    { "some-data", new[] { "value1", "value2" } }
                },
            };
            var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status},\"detail\":\"{value.Detail}\",\"instance\":\"{JsonEncodedText.Encode(value.Instance)}\",\"traceId\":\"{traceId}\",\"some-data\":[\"value1\",\"value2\"],\"errors\":{{\"property0\":[\"error0\"],\"property1\":[\"error1\",\"error2\"]}}}}";
            var converter = new ValidationProblemDetailsJsonConverter();
            var stream = new MemoryStream();

            // Act
            using (var writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, value, JsonSerializerOptions);
            }

            // Assert
            var actual = Encoding.UTF8.GetString(stream.ToArray());
            actual.Should().Be(expected);
        }

        [Test]
        public void Write_WithSomeMissingContent_Works()
        {
            // Arrange
            var value = new ValidationProblemDetails
            {
                Title = "Not found",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 404,
            };
            var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status},\"errors\":{{}}}}";
            var converter = new ValidationProblemDetailsJsonConverter();
            var stream = new MemoryStream();

            // Act
            using (var writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, value, JsonSerializerOptions);
            }

            // Assert
            var actual = Encoding.UTF8.GetString(stream.ToArray());
            actual.Should().Be(expected);
        }

        [Test]
        public void Write_WithNull_PropertyNamingPolicy_Works()
        {
            // Arrange
            var options = BlueprintJsonOptions.CreateOptions();
            options.PropertyNamingPolicy = null;

            var value = new ValidationProblemDetails(new Dictionary<string, IEnumerable<string>>
            {
                ["Property1"] = new [] { "Error 1" },
            })
            {
                Title = "Validation error",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 400,
            };
            var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status},\"errors\":{{\"Property1\":[\"Error 1\"]}}}}";
            var converter = new ValidationProblemDetailsJsonConverter();
            var stream = new MemoryStream();

            // Act
            using (var writer = new Utf8JsonWriter(stream))
            {
                converter.Write(writer, value, options);
            }

            // Assert
            var actual = Encoding.UTF8.GetString(stream.ToArray());
            actual.Should().Be(expected);
        }
    }
}
