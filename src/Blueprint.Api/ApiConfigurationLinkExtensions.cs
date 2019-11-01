using System;
using System.Collections.Concurrent;
using Blueprint.Api.Configuration;
using Blueprint.Core.ThirdParty;

namespace Blueprint.Api
{
    public static class ApiConfigurationLinkExtensions
    {
        private static readonly ConcurrentDictionary<string, string> UrlMappings = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Given a resource type and it's Id generates a <see cref="Link"/> that maps to it and can be output to
        /// <see cref="ApiResource.Links"/>.
        /// </summary>
        /// <param name="configuration">The configuration of the API, used to grab <see cref="BlueprintApiOptions.BaseApiUrl"/>.</param>
        /// <param name="id">The id of the resource being linked to.</param>
        /// <typeparam name="T">The <see cref="ApiResource"/> type.</typeparam>
        /// <returns>A new <see cref="Link"/> that references the given resource.</returns>
        public static Link LinkFor<T>(this BlueprintApiOptions configuration, int id)
        {
            return new Link
            {
                Type = typeof(T).Name,
                Href = BuildBaseUrl(configuration, typeof(T)) + "/" + id,
            };
        }

        // TODO: We should be looking up base URL from ApiDataModel somehow
        private static string BuildBaseUrl(BlueprintApiOptions configuration, Type resourceType)
        {
            return
                configuration.BaseApiUrl +
                UrlMappings.GetOrAdd(resourceType.Name, x => resourceType.Name.Pluralize().Camelize());
        }
    }
}
