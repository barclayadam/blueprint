// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// A default implementation of <see cref="IOutputFormatterSelector" /> that inspects the
    /// <c>Accept</c> of requests and uses the <see cref="IOperationResultOutputFormatter.IsSupported" /> method
    /// to determine the most appropriate <see cref="IOperationResultOutputFormatter" /> to use.
    /// </summary>
    public class DefaultOutputFormatterSelector : IOutputFormatterSelector
    {
        private static readonly Comparison<MediaTypeSegmentWithQuality> sortFunction = (left, right) =>
            left.Quality > right.Quality ? -1 : left.Quality == right.Quality ? 0 : 1;

        private readonly ILogger logger;
        private readonly IList<IOperationResultOutputFormatter> formatters;
        private readonly bool respectBrowserAcceptHeader;
        private readonly bool returnHttpNotAcceptable;

        public DefaultOutputFormatterSelector(IOptions<BlueprintHttpOptions> options, ILoggerFactory loggerFactory)
        {
            Guard.NotNull(nameof(options), options);
            Guard.NotNull(nameof(loggerFactory), loggerFactory);

            logger = loggerFactory.CreateLogger<DefaultOutputFormatterSelector>();

            formatters = new ReadOnlyCollection<IOperationResultOutputFormatter>(options.Value.OutputFormatters);
            respectBrowserAcceptHeader = options.Value.RespectBrowserAcceptHeader;
            returnHttpNotAcceptable = options.Value.ReturnHttpNotAcceptable;
        }

        /// <inheritdoc />
        public IOperationResultOutputFormatter SelectFormatter(OutputFormatterCanWriteContext context)
        {
            Guard.NotNull(nameof(context), context);

            var acceptableMediaTypes = GetAcceptableMediaTypes(context.Request);
            var selectFormatterWithoutRegardingAcceptHeader = false;

            IOperationResultOutputFormatter selectedFormatter = null;

            if (acceptableMediaTypes.Count == 0)
            {
                // There is either no Accept header value, or it contained */* and we
                // are not currently respecting the 'browser accept header'.
                logger.LogDebug("No information found on request to perform content negotiation.");

                selectFormatterWithoutRegardingAcceptHeader = true;
            }
            else
            {
                logger.LogDebug("Attempting to select an output formatter based on Accept header '{AcceptHeader}'.", acceptableMediaTypes);

                // Use whatever formatter can meet the client's request
                selectedFormatter = SelectFormatterUsingSortedAcceptHeaders(
                    context,
                    acceptableMediaTypes);

                if (selectedFormatter == null)
                {
                    logger.LogDebug(
                        "Could not find an output formatter based on content negotiation. Accepted types were ({AcceptTypes})",
                        acceptableMediaTypes);

                    if (!returnHttpNotAcceptable)
                    {
                        selectFormatterWithoutRegardingAcceptHeader = true;
                    }
                }
            }

            if (selectFormatterWithoutRegardingAcceptHeader)
            {
                logger.LogDebug(
                    "Attempting to select an output formatter without using a content type as no explicit content types were specified for the response.");

                selectedFormatter = SelectFormatterNotUsingContentType(context);
            }

            if (selectedFormatter != null && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Selected output formatter '{OutputFormatter}' to write the response.",
                    selectedFormatter);
            }

            return selectedFormatter;
        }

        private List<MediaTypeSegmentWithQuality> GetAcceptableMediaTypes(HttpRequest request)
        {
            var result = new List<MediaTypeSegmentWithQuality>();
            AcceptHeaderParser.ParseAcceptHeader(request.Headers[HeaderNames.Accept], result);
            for (var i = 0; i < result.Count; i++)
            {
                var mediaType = new MediaType(result[i].MediaType);
                if (!respectBrowserAcceptHeader && mediaType.MatchesAllSubTypes && mediaType.MatchesAllTypes)
                {
                    result.Clear();
                    return result;
                }
            }

            result.Sort(sortFunction);

            return result;
        }

        private IOperationResultOutputFormatter SelectFormatterNotUsingContentType(OutputFormatterCanWriteContext formatterContext)
        {
            logger.LogDebug("Attempting to select the first formatter in the output formatters list which can write the result.");

            foreach (var formatter in formatters)
            {
                formatterContext.ContentType = default;

                if (formatter.IsSupported(formatterContext))
                {
                    return formatter;
                }
            }

            return null;
        }

        private IOperationResultOutputFormatter SelectFormatterUsingSortedAcceptHeaders(
            OutputFormatterCanWriteContext formatterContext,
            IList<MediaTypeSegmentWithQuality> sortedAcceptHeaders)
        {
            for (var i = 0; i < sortedAcceptHeaders.Count; i++)
            {
                var mediaType = sortedAcceptHeaders[i];

                formatterContext.ContentType = new MediaType(mediaType.MediaType);

                for (var j = 0; j < formatters.Count; j++)
                {
                    var formatter = formatters[j];

                    if (formatter.IsSupported(formatterContext))
                    {
                        return formatter;
                    }
                }
            }

            return null;
        }
    }
}
