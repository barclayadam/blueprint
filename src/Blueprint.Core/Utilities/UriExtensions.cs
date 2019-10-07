using System;
using System.Web;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="Uri" /> and <see cref="UriBuilder" />.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Sets the specified parameter in the Query String of the URI.
        /// </summary>
        /// <param name="url">The url to set query param to.</param>
        /// <param name="paramName">Name of the parameter to add.</param>
        /// <param name="paramValue">Value for the parameter to add.</param>
        /// <returns>Url with added parameter.</returns>
        public static Uri SetQueryParameter(this Uri url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            uriBuilder.SetQueryParameter(paramName, paramValue);

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Sets the specified parameter in the Query String of the URI of this builder.
        /// </summary>
        /// <param name="uriBuilder">The builder to set query param on.</param>
        /// <param name="paramName">Name of the parameter to add.</param>
        /// <param name="paramValue">Value for the parameter to add.</param>
        public static void SetQueryParameter(this UriBuilder uriBuilder, string paramName, string paramValue)
        {
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[paramName] = paramValue;
            uriBuilder.Query = query.ToString();
        }
    }
}
