using System.Collections.Generic;
using Blueprint.Http.Formatters;

namespace Blueprint.Http
{
    /// <summary>
    /// A set of options that are used throughout the Blueprint HTTP support module.
    /// </summary>
    public class BlueprintHttpOptions
    {
        /// <summary>
        /// The list of output formatters available.
        /// </summary>
        public List<IOperationResultOutputFormatter> OutputFormatters { get; } = new List<IOperationResultOutputFormatter>();

        /// <summary>
        /// Gets or sets the flag which causes content negotiation to ignore Accept header
        /// when it contains the media type */*. <see langword="false"/> by default.
        /// </summary>
        public bool RespectBrowserAcceptHeader { get; set; }

        /// <summary>
        /// Gets or sets the flag which decides whether an HTTP 406 Not Acceptable response
        /// will be returned if no formatter has been selected to format the response.
        /// <see langword="false"/> by default.
        /// </summary>
        public bool ReturnHttpNotAcceptable { get; set; }
    }
}
