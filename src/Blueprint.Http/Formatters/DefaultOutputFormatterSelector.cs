// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private static readonly Comparison<MediaTypeSegmentWithQuality> _sortFunction = (left, right) =>
            left.Quality > right.Quality ? -1 : left.Quality == right.Quality ? 0 : 1;

        private readonly ILogger _logger;
        private readonly List<IOperationResultOutputFormatter> _formatters;
        private readonly bool _respectBrowserAcceptHeader;
        private readonly bool _returnHttpNotAcceptable;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultOutputFormatterSelector"/> class.
        /// </summary>
        /// <param name="options">The configured HTTP options.</param>
        /// <param name="logger">A logger for this class.</param>
        public DefaultOutputFormatterSelector(IOptions<BlueprintHttpOptions> options, ILogger<DefaultOutputFormatterSelector> logger)
        {
            Guard.NotNull(nameof(options), options);
            Guard.NotNull(nameof(logger), logger);

            this._logger = logger;
            this._formatters = options.Value.OutputFormatters;
            this._respectBrowserAcceptHeader = options.Value.RespectBrowserAcceptHeader;
            this._returnHttpNotAcceptable = options.Value.ReturnHttpNotAcceptable;
        }

        /// <inheritdoc />
        public IOperationResultOutputFormatter SelectFormatter(OutputFormatterCanWriteContext context)
        {
            Guard.NotNull(nameof(context), context);

            var acceptableMediaTypes = this.GetAcceptableMediaTypes(context.Request);
            var selectFormatterWithoutRegardingAcceptHeader = false;

            IOperationResultOutputFormatter selectedFormatter = null;

            if (acceptableMediaTypes.Count == 0)
            {
                // There is either no Accept header value, or it contained */* and we
                // are not currently respecting the 'browser accept header'.
                this._logger.LogDebug("No information found on request to perform content negotiation.");

                selectFormatterWithoutRegardingAcceptHeader = true;
            }
            else
            {
                this._logger.LogDebug("Attempting to select an output formatter based on Accept header '{AcceptHeader}'.", acceptableMediaTypes);

                // Use whatever formatter can meet the client's request
                selectedFormatter = this.SelectFormatterUsingSortedAcceptHeaders(
                    context,
                    acceptableMediaTypes);

                if (selectedFormatter == null)
                {
                    this._logger.LogDebug(
                        "Could not find an output formatter based on content negotiation. Accepted types were ({AcceptTypes})",
                        acceptableMediaTypes);

                    if (!this._returnHttpNotAcceptable)
                    {
                        selectFormatterWithoutRegardingAcceptHeader = true;
                    }
                }
            }

            if (selectFormatterWithoutRegardingAcceptHeader)
            {
                this._logger.LogDebug(
                    "Attempting to select an output formatter without using a content type as no explicit content types were specified for the response.");

                selectedFormatter = this.SelectFormatterNotUsingContentType(context);
            }

            if (selectedFormatter != null && this._logger.IsEnabled(LogLevel.Debug))
            {
                this._logger.LogDebug(
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
                if (!this._respectBrowserAcceptHeader && mediaType.MatchesAllSubTypes && mediaType.MatchesAllTypes)
                {
                    result.Clear();
                    return result;
                }
            }

            result.Sort(_sortFunction);

            return result;
        }

        private IOperationResultOutputFormatter SelectFormatterNotUsingContentType(OutputFormatterCanWriteContext formatterContext)
        {
            this._logger.LogDebug("Attempting to select the first formatter in the output formatters list which can write the result.");

            foreach (var formatter in this._formatters)
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
            foreach (var mediaType in sortedAcceptHeaders)
            {
                formatterContext.ContentType = new MediaType(mediaType.MediaType);

                foreach (var formatter in this._formatters)
                {
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
