using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blueprint.Core.Utilities
{
    public static class HttpExtensions
    {
        private static readonly Dictionary<string, string> EmptyLinkDictionary = new Dictionary<string, string>();

        private static readonly Regex LinksHeaderUrl = new Regex("<(.*)>", RegexOptions.Compiled);
        private static readonly Regex LinksHeaderRel = new Regex("rel=(\"|')(.*)\\1", RegexOptions.Compiled);

        /// <summary>
        /// Given the 'Links' header will parse the links that have been returned from the API.
        /// </summary>
        /// <param name="linksHeader">The links header, which may be null or contain an empty string.</param>
        /// <returns>A dictionary of links, with the key being the <c>rel</c> and the value the URL.</returns>
        public static IReadOnlyDictionary<string, string> ParseLinks(string linksHeader)
        {
            if (string.IsNullOrEmpty(linksHeader))
            {
                return EmptyLinkDictionary;
            }

            var entries = linksHeader.Split(',');

            return entries.ToDictionary(
                    e => LinksHeaderRel.Match(e).Groups[2].Value,
                    e => LinksHeaderUrl.Match(e).Groups[1].Value);
        }

        public static (string Url, string Rel) ParseLink(string link)
        {
            return (
                LinksHeaderRel.Match(link).Groups[2].Value,
                LinksHeaderUrl.Match(link).Groups[1].Value
            );
        }
    }
}
