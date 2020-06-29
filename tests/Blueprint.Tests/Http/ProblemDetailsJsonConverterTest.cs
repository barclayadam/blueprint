// Originally from https://github.com/dotnet/aspnetcore/blob/602cb1dea59751a1297d674cddf7f0729c23e6ca/src/Mvc/Mvc.Core/test/Infrastructure/ProblemDetailsJsonConverterTest.cs
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Blueprint.Http;
using Blueprint.Http.Formatters;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Http
{
    public class ProblemDetailsJsonConverterTest
    {
        private static JsonSerializerOptions JsonSerializerOptions => JsonOperationResultOutputFormatter.JsonSerializerOptions;

        [Test]
        public void Read_ThrowsIfJsonIsIncomplete()
        {
            // Arrange
            var json = "{";
            var converter = new ProblemDetailsJsonConverter();

            // Act & Assert
            Action convert = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                converter.Read(ref reader, typeof(ProblemDetails), JsonSerializerOptions);
            };

            convert.Should().ThrowExactly<JsonException>();
        }

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
            var json =
                $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"detail\":\"{detail}\", \"instance\":\"{instance}\",\"traceId\":\"{traceId}\"}}";
            var converter = new ProblemDetailsJsonConverter();
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var problemDetails = converter.Read(ref reader, typeof(ProblemDetails), JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Instance.Should().Be(instance);
            problemDetails.Detail.Should().Be(detail);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
        }

        [Test]
        public void Read_UsingJsonSerializerWorks()
        {
            // Arrange
            var type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            var title = "Not found";
            var status = 404;
            var detail = "Product not found";
            var instance = "http://example.com/products/14";
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var json =
                $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"detail\":\"{detail}\", \"instance\":\"{instance}\",\"traceId\":\"{traceId}\"}}";

            // Act
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(json, JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Instance.Should().Be(instance);
            problemDetails.Detail.Should().Be(detail);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
        }

        [Test]
        public void Read_WithSomeMissingValues_Works()
        {
            // Arrange
            var type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            var title = "Not found";
            var status = 404;
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"}}";
            var converter = new ProblemDetailsJsonConverter();
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var problemDetails = converter.Read(ref reader, typeof(ProblemDetails), JsonSerializerOptions);

            problemDetails.Type.Should().Be(type);
            problemDetails.Title.Should().Be(title);
            problemDetails.Status.Should().Be(status);
            problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
        }

        [Test]
        public void Write_Works()
        {
            // Arrange
            var traceId = "|37dd3dd5-4a9619f953c40a16.";
            var value = new ProblemDetails
            {
                Title = "Not found",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 404,
                Detail = "Product not found",
                Instance = "http://example.com/products/14",
                Extensions =
                {
                    { "traceId", traceId },
                    { "some-data", new[] { "value1", "value2" } }
                }
            };
            var expected =
                $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status},\"detail\":\"{value.Detail}\",\"instance\":\"{JsonEncodedText.Encode(value.Instance)}\",\"traceId\":\"{traceId}\",\"some-data\":[\"value1\",\"value2\"]}}";
            var converter = new ProblemDetailsJsonConverter();
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
            var value = new ProblemDetails
            {
                Title = "Not found",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 404,
            };
            var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status}}}";
            var converter = new ProblemDetailsJsonConverter();
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
    }
}
