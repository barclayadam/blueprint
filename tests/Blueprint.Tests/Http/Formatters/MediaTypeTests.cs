// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Blueprint.Http.Formatters;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Blueprint.Tests.Http.Formatters
{
    public class MediaTypeTest
    {
        [Test]
        [TestCase("application/json")]
        [TestCase("application /json")]
        [TestCase(" application / json ")]
        public void Constructor_CanParseParameterlessSuffixlessMediaTypes(string mediaType)
        {
            // Arrange & Act
            var result = new MediaType(mediaType, 0, mediaType.Length);

            // Assert
            new StringSegment("application").Should().Be(result.Type);
            new StringSegment("json").Should().Be(result.SubType);
        }

        public static IEnumerable<object[]> MediaTypesWithSuffixes
        {
            get
            {
                return new List<string[]>
                {
                    // See https://tools.ietf.org/html/rfc6838#section-4.2 for allowed names spec
                    new[] { "application/json", "json", null },
                    new[] { "application/json+", "json", "" },
                    new[] { "application/+json", "", "json" },
                    new[] { "application/entitytype+json", "entitytype", "json" },
                    new[] { "  application /  vnd.com-pany.some+entity!.v2+js.#$&^_n  ; q=\"0.3+1\"", "vnd.com-pany.some+entity!.v2", "js.#$&^_n" },
                };
            }
        }

        [Test]
        [TestCaseSource(nameof(MediaTypesWithSuffixes))]
        public void Constructor_CanParseSuffixedMediaTypes(
            string mediaType,
            string expectedSubTypeWithoutSuffix,
            string expectedSubtypeSuffix)
        {
            // Arrange & Act
            var result = new MediaType(mediaType);

            // Assert
            new StringSegment(expectedSubTypeWithoutSuffix).Should().Be(result.SubTypeWithoutSuffix);
            new StringSegment(expectedSubtypeSuffix).Should().Be(result.SubTypeSuffix);
        }

        public static IEnumerable<string> MediaTypesWithParameters
        {
            get
            {
                return new string[]
                {
                    "application/json+bson;format=pretty;charset=utf-8;q=0.8",
                    "application/json+bson;format=pretty;charset=\"utf-8\";q=0.8",
                    "application/json+bson;format=pretty;charset=utf-8; q=0.8 ",
                    "application/json+bson;format=pretty;charset=utf-8 ; q=0.8 ",
                    "application/json+bson;format=pretty; charset=utf-8 ; q=0.8 ",
                    "application/json+bson;format=pretty ; charset=utf-8 ; q=0.8 ",
                    "application/json+bson; format=pretty ; charset=utf-8 ; q=0.8 ",
                    "application/json+bson; format=pretty ; charset=utf-8 ; q=  0.8 ",
                    "application/json+bson; format=pretty ; charset=utf-8 ; q  =  0.8 ",
                    " application /  json+bson; format =  pretty ; charset = utf-8 ; q  =  0.8 ",
                    " application /  json+bson; format =  \"pretty\" ; charset = \"utf-8\" ; q  =  \"0.8\" ",
                };
            }
        }

        [Test]
        [TestCaseSource(nameof(MediaTypesWithParameters))]
        public void Constructor_CanParseMediaTypesWithParameters(string mediaType)
        {
            // Arrange & Act
            var result = new MediaType(mediaType, 0, mediaType.Length);

            // Assert
            new StringSegment("application").Should().Be(result.Type);
            new StringSegment("json+bson").Should().Be(result.SubType);
            new StringSegment("json").Should().Be(result.SubTypeWithoutSuffix);
            new StringSegment("bson").Should().Be(result.SubTypeSuffix);
            new StringSegment("pretty").Should().Be(result.GetParameter("format"));
            new StringSegment("0.8").Should().Be(result.GetParameter("q"));
            new StringSegment("utf-8").Should().Be(result.GetParameter("charset"));
        }

        [Test]
        public void Constructor_NullLength_IgnoresLength()
        {
            // Arrange & Act
            var result = new MediaType("mediaType", 1, length: null);

            // Assert
            new StringSegment("ediaType").Should().Be(result.Type);
        }

        [Test]
        public void Constructor_NullMediaType_Throws()
        {
            // Arrange, Act and Assert
            Assert.Throws<ArgumentNullException>(() => new MediaType(null, 0, 2));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(7)]
        public void Constructor_NegativeOffset_Throws(int offset)
        {
            // Arrange, Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new MediaType("media", offset, 5));
        }

        [Test]
        public void Constructor_NegativeLength_Throws()
        {
            // Arrange, Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new MediaType("media", 0, -1));
        }

        [Test]
        public void Constructor_OffsetOrLengthOutOfBounds_Throws()
        {
            // Arrange, Act and Assert
            Assert.Throws<ArgumentException>(() => new MediaType("lengthof9", 5, 5));
        }

        [Test]
        [TestCaseSource(nameof(MediaTypesWithParameters))]
        public void ReplaceEncoding_ReturnsExpectedMediaType(string mediaType)
        {
            // Arrange
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var expectedMediaType = mediaType.Replace("utf-8", "iso-8859-1");

            // Act
            var result = MediaType.ReplaceEncoding(mediaType, encoding);

            // Assert
            expectedMediaType.Should().Be(result);
        }

        [Test]
        [TestCase("application/json;charset=utf-8")]
        [TestCase("application/json;format=indent;q=0.8;charset=utf-8")]
        [TestCase("application/json;format=indent;charset=utf-8;q=0.8")]
        [TestCase("application/json;charset=utf-8;format=indent;q=0.8")]
        public void GetParameter_ReturnsParameter_IfParameterIsInMediaType(string mediaType)
        {
            // Arrange
            var expectedParameter = new StringSegment("utf-8");
            var parsedMediaType = new MediaType(mediaType, 0, mediaType.Length);

            // Act
            var result = parsedMediaType.GetParameter("charset");

            // Assert
            expectedParameter.Should().Be(result);
        }

        [Test]
        public void GetParameter_ReturnsNull_IfParameterIsNotInMediaType()
        {
            var mediaType = "application/json;charset=utf-8;format=indent;q=0.8";

            var parsedMediaType = new MediaType(mediaType, 0, mediaType.Length);

            // Act
            var result = parsedMediaType.GetParameter("other");

            // Assert
            Assert.False(result.HasValue);
        }

        [Test]
        public void GetParameter_IsCaseInsensitive()
        {
            // Arrange
            var mediaType = "application/json;charset=utf-8";
            var expectedParameter = new StringSegment("utf-8");

            var parsedMediaType = new MediaType(mediaType);

            // Act
            var result = parsedMediaType.GetParameter("CHARSET");

            // Assert
            expectedParameter.Should().Be(result);
        }

        [Test]
        [TestCase("application/json", "application/json")]
        [TestCase("application/json", "application/json;charset=utf-8")]
        [TestCase("application/json;q=0.8", "application/json;q=0.9")]
        [TestCase("application/json;q=0.8;charset=utf-7", "application/json;charset=utf-8;q=0.9")]
        [TestCase("application/json", "application/json;format=indent;charset=utf-8")]
        [TestCase("application/json;format=indent;charset=utf-8", "application/json;format=indent;charset=utf-8")]
        [TestCase("application/json;charset=utf-8;format=indent", "application/json;format=indent;charset=utf-8")]
        [TestCase("application/*", "application/json")]
        [TestCase("application/*", "application/entitytype+json;v=2")]
        [TestCase("application/*;v=2", "application/entitytype+json;v=2")]
        [TestCase("application/json;*", "application/json;v=2")]
        [TestCase("application/json;v=2;*", "application/json;v=2;charset=utf-8")]
        [TestCase("*/*", "application/json")]
        [TestCase("application/entity+json", "application/entity+json")]
        [TestCase("application/*+json", "application/entity+json")]
        [TestCase("application/*", "application/entity+json")]
        [TestCase("application/json", "application/vnd.restful+json")]
        [TestCase("application/json", "application/problem+json")]
        public void IsSubsetOf_ReturnsTrueWhenExpected(string set, string subset)
        {
            // Arrange
            var setMediaType = new MediaType(set);
            var subSetMediaType = new MediaType(subset);

            // Act
            var result = subSetMediaType.IsSubsetOf(setMediaType);

            // Assert
            Assert.True(result);
        }

        [Test]
        [TestCase("application/json;charset=utf-8", "application/json")]
        [TestCase("application/json;format=indent;charset=utf-8", "application/json")]
        [TestCase("application/json;format=indent;charset=utf-8", "application/json;charset=utf-8")]
        [TestCase("application/*", "text/json")]
        [TestCase("application/*;v=2", "application/json")]
        [TestCase("application/*;v=2", "application/json;v=1")]
        [TestCase("application/json;v=2;*", "application/json;v=1")]
        [TestCase("application/entity+json", "application/entity+txt")]
        [TestCase("application/entity+json", "application/entity.v2+json")]
        [TestCase("application/*+json", "application/entity+txt")]
        [TestCase("application/entity+*", "application/entity.v2+json")]
        [TestCase("application/*+*", "application/json")]
        [TestCase("application/entity+*", "application/entity+json")] // We don't allow suffixes to be wildcards
        [TestCase("application/*+*", "application/entity+json")] // We don't allow suffixes to be wildcards
        [TestCase("application/entity+json", "application/entity")]
        public void IsSubsetOf_ReturnsFalseWhenExpected(string set, string subset)
        {
            // Arrange
            var setMediaType = new MediaType(set);
            var subSetMediaType = new MediaType(subset);

            // Act
            var result = subSetMediaType.IsSubsetOf(setMediaType);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void MatchesAllTypes_ReturnsTrueWhenExpected()
        {
            // Arrange
            var mediaType = new MediaType("*/*");

            // Act
            var result = mediaType.MatchesAllTypes;

            // Assert
            Assert.True(result);
        }

        [Test]
        [TestCase("text/*")]
        [TestCase("text/plain")]
        public void MatchesAllTypes_ReturnsFalseWhenExpected(string value)
        {
            // Arrange
            var mediaType = new MediaType(value);

            // Act
            var result = mediaType.MatchesAllTypes;

            // Assert
            Assert.False(result);
        }

        [Test]
        [TestCase("*/*")]
        [TestCase("text/*")]
        public void MatchesAllSubtypes_ReturnsTrueWhenExpected(string value)
        {
            // Arrange
            var mediaType = new MediaType(value);

            // Act
            var result = mediaType.MatchesAllSubTypes;

            // Assert
            Assert.True(result);
        }

        [Test]
        public void MatchesAllSubtypes_ReturnsFalseWhenExpected()
        {
            // Arrange
            var mediaType = new MediaType("text/plain");

            // Act
            var result = mediaType.MatchesAllSubTypes;

            // Assert
            Assert.False(result);
        }

        [Test]
        [TestCase("*/*", true)]
        [TestCase("text/*", true)]
        [TestCase("text/*+suffix", true)]
        [TestCase("text/*+", true)]
        [TestCase("text/*+*", true)]
        [TestCase("text/json+suffix", false)]
        [TestCase("*/json+*", false)]
        public void MatchesAllSubTypesWithoutSuffix_ReturnsExpectedResult(string value, bool expectedReturnValue)
        {
            // Arrange
            var mediaType = new MediaType(value);

            // Act
            var result = mediaType.MatchesAllSubTypesWithoutSuffix;

            // Assert
            expectedReturnValue.Should().Be(result);
        }

        [Test]
        [TestCase("*/*", true)]
        [TestCase("text/*", true)]
        [TestCase("text/entity+*", false)] // We don't support wildcards on suffixes
        [TestCase("text/*+json", true)]
        [TestCase("text/entity+json;*", true)]
        [TestCase("text/entity+json;v=3;*", true)]
        [TestCase("text/entity+json;v=3;q=0.8", false)]
        [TestCase("text/json", false)]
        [TestCase("text/json;param=*", false)] // * is the literal value of the param
        public void HasWildcard_ReturnsTrueWhenExpected(string value, bool expectedReturnValue)
        {
            // Arrange
            var mediaType = new MediaType(value);

            // Act
            var result = mediaType.HasWildcard;

            // Assert
            expectedReturnValue.Should().Be(result);
        }

        [Test]
        [TestCaseSource(nameof(MediaTypesWithParameters))]
        [TestCase("application/json;format=pretty;q=0.9;charset=utf-8;q=0.8")]
        [TestCase("application/json;format=pretty;q=0.9;charset=utf-8;q=0.8;version=3")]
        public void CreateMediaTypeSegmentWithQuality_FindsQValue(string value)
        {
            // Arrange & Act
            var mediaTypeSegment = MediaType.CreateMediaTypeSegmentWithQuality(value, start: 0);

            // Assert
            0.8d.Should().Be(mediaTypeSegment.Quality);
        }
    }
}
