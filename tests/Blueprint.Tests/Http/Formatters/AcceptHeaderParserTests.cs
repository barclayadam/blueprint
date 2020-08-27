// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Http.Formatters;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Blueprint.Tests.Http.Formatters
{
    public class AcceptHeaderParserTest
    {
        [Test]
        public void ParseAcceptHeader_ParsesSimpleHeader()
        {
            // Arrange
            var header = "application/json";
            var expected = new List<MediaTypeSegmentWithQuality>
            {
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"),1.0)
            };

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string> { header });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_ParsesSimpleHeaderWithMultipleValues()
        {
            // Arrange
            var header = "application/json, application/xml;q=0.8";
            var expected = new List<MediaTypeSegmentWithQuality>
            {
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"),1.0),
                new MediaTypeSegmentWithQuality(new StringSegment("application/xml;q=0.8"),0.8)
            };

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string> { header });

            // Assert
            expected.Should().Equal(parsed);

            foreach (var mediaType in parsed)
            {
                header.Should().Be(mediaType.MediaType.Buffer);
            }
        }

        [Test]
        public void ParseAcceptHeader_ParsesSimpleHeaderWithMultipleValues_InvalidFormat()
        {
            // Arrange
            var header = "application/json, application/xml,;q=0.8";
            var expectedMediaTypes = new List<MediaTypeSegmentWithQuality>
            {
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"),1.0),
                new MediaTypeSegmentWithQuality(new StringSegment("application/xml"),1.0),
            };

            // Act
            var mediaTypes = AcceptHeaderParser.ParseAcceptHeader(new List<string> { header });

            // Assert
            expectedMediaTypes.Should().BeEquivalentTo(mediaTypes);
        }

        [Test]
        [TestCase(";q=0.9", "")]
        [TestCase("/", "")]
        [TestCase("*/", "")]
        [TestCase("/*", "")]
        [TestCase("/;q=0.9", "")]
        [TestCase("*/;q=0.9", "")]
        [TestCase("/*;q=0.9", "")]
        [TestCase("/;q=0.9,text/html", "text/html")]
        [TestCase("*/;q=0.9,text/html", "text/html")]
        [TestCase("/*;q=0.9,text/html", "text/html")]
        [TestCase("img/png,/;q=0.9,text/html", "img/png,text/html")]
        [TestCase("img/png,*/;q=0.9,text/html", "img/png,text/html")]
        [TestCase("img/png,/*;q=0.9,text/html", "img/png,text/html")]
        [TestCase("img/png,/;q=0.9", "img/png")]
        [TestCase("img/png,*/;q=0.9", "img/png")]
        [TestCase("img/png;q=1.0, /*;q=0.9", "img/png;q=1.0")]
        public void ParseAcceptHeader_GracefullyRecoversFromInvalidMediaTypeValues_AndReturnsValidMediaTypes(
            string acceptHeader,
            string expected)
        {
            // Arrange
            var expectedMediaTypes = expected
                .Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => new MediaTypeSegmentWithQuality(new StringSegment(e), 1.0))
                .ToList();

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(acceptHeader.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries));

            // Assert
            expectedMediaTypes.Should().BeEquivalentTo(parsed);
        }

        [Test]
        public void ParseAcceptHeader_ParsesMultipleHeaderValues()
        {
            // Arrange
            var expected = new List<MediaTypeSegmentWithQuality>
            {
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"), 1.0),
                new MediaTypeSegmentWithQuality(new StringSegment("application/xml;q=0.8"), 0.8)
            };

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(
                new List<string> { "application/json", "", "application/xml;q=0.8" });

            // Assert
            expected.Should().Equal(parsed);
        }

        // The text "*/*Content-Type" parses as a valid media type value. However it's followed
        // by ':' instead of whitespace or a delimiter, which means that it's actually invalid.
        [Test]
        public void ParseAcceptHeader_ValidMediaType_FollowedByNondelimiter()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[0];

            var input = "*/*Content-Type:application/json";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_ValidMediaType_FollowedBySemicolon()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[0];

            var input = "*/*Content-Type;application/json";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_ValidMediaType_FollowedByComma()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[]
            {
                new MediaTypeSegmentWithQuality(new StringSegment("*/*Content-Type"), 1.0),
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"), 1.0),
            };

            var input = "*/*Content-Type,application/json";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_ValidMediaType_FollowedByWhitespace()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[]
            {
                new MediaTypeSegmentWithQuality(new StringSegment("application/json"), 1.0),
            };

            var input = "*/*Content-Type application/json";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_InvalidTokenAtStart()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[0];

            var input = ":;:";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_DelimiterAtStart()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[0];

            var input = ",;:";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }

        [Test]
        public void ParseAcceptHeader_InvalidTokenAtEnd()
        {
            // Arrange
            var expected = new MediaTypeSegmentWithQuality[0];

            var input = "*/*:";

            // Act
            var parsed = AcceptHeaderParser.ParseAcceptHeader(new List<string>() { input });

            // Assert
            expected.Should().Equal(parsed);
        }
    }
}
