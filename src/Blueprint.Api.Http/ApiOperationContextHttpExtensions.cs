using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// Extensions methods to <see cref="ApiOperationContext" /> to manage the storage of a <see cref="HttpContext" />.
    /// </summary>
    public static class ApiOperationContextHttpExtensions
    {
        /// <summary>
        /// Gets the <see cref="HttpContext" /> that has been registered with this <see cref="ApiOperationContext" />.
        /// </summary>
        /// <param name="context">The context to load from.</param>
        /// <returns>The <see cref="HttpContext"/> registered.</returns>
        /// <exception cref="InvalidOperationException">If no <see cref="HttpContext"/> has been registered.</exception>
        /// <seealso cref="SetHttpFeatureContext" />
        public static HttpContext GetHttpContext(this ApiOperationContext context)
        {
            return GetHttpFeatureContext(context).HttpContext;
        }

        /// <summary>
        /// Gets the <see cref="HttpContext" /> that has been registered with this <see cref="ApiOperationContext" />.
        /// </summary>
        /// <param name="context">The context to load from.</param>
        /// <returns>The <see cref="HttpContext"/> registered.</returns>
        /// <exception cref="InvalidOperationException">If no <see cref="HttpContext"/> has been registered.</exception>
        /// <seealso cref="SetHttpFeatureContext" />
        public static RouteData GetRouteData(this ApiOperationContext context)
        {
            return GetHttpFeatureContext(context).RouteData;
        }

        /// <summary>
        /// Gets the <see cref="RouteContext" /> that has been registered with this <see cref="ApiOperationContext" />.
        /// </summary>
        /// <param name="context">The context to load from.</param>
        /// <returns>The <see cref="RouteContext"/> registered.</returns>
        /// <exception cref="InvalidOperationException">If no <see cref="RouteContext"/> has been registered.</exception>
        /// <seealso cref="SetHttpFeatureContext" />
        public static HttpFeatureContext GetHttpFeatureContext(this ApiOperationContext context)
        {
            if (context.Data.TryGetValue(nameof(HttpFeatureContext), out var value))
            {
                return (HttpFeatureContext)value;
            }

            throw new InvalidOperationException(
                $"A HttpFeatureContext instance does not exist on this {nameof(ApiOperationContext)}. To use HTTP-specific features a HttpContext must exist, which " +
                $"can be accomplished by using {nameof(ApplicationBuilderExtensions.UseBlueprintApi)} in your ASP.NET Core web application");
        }

        /// <summary>
        /// Stores the given <see cref="RouteContext" /> with this <see cref="ApiOperationContext" />.
        /// </summary>
        /// <param name="context">The context on which to store the HTTP context.</param>
        /// <param name="routeContext">The route context to store.</param>
        /// <seealso cref="GetHttpContext" />
        /// <seealso cref="GetRouteData" />
        public static void SetHttpFeatureContext(this ApiOperationContext context, HttpFeatureContext httpFeature)
        {
            context.Data[nameof(HttpFeatureContext)] = httpFeature;
        }
    }
}
