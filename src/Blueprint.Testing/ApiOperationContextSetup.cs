using System;
using System.Net;
using Blueprint.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Testing
{
    /// <summary>
    /// Provides methods that can be used to create and configure <see cref="ApiOperationContext"/>s for usage within
    /// unit and integration tests.
    /// </summary>
    public static class ApiOperationContextSetup
    {
        /// <summary>
        /// Sets the properties of <see cref="HttpRequest" /> of the given context to match those from
        /// the specified URL.
        /// </summary>
        /// <param name="httpContext">The HttpContext to configure.</param>
        /// <param name="url">The URL to set.</param>
        public static void SetRequestUri(this DefaultHttpContext httpContext, string url)
        {
            var uri = new Uri(url);

            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Path = new PathString(uri.LocalPath);
            httpContext.Request.QueryString = new QueryString(uri.Query);
        }

        /// <summary>
        /// Configures the <see cref="ApiOperationContext" /> with a HTTP context configured for the given URL.
        /// </summary>
        /// <remarks>
        /// In addition to adding the <see cref="RouteContext"/> on the descriptor this method will set the newly
        /// created <see cref="HttpContext" /> on the <see cref="HttpContextAccessor" /> that has been registered
        /// with the context's <see cref="IServiceProvider"/>.
        /// </remarks>
        /// <param name="context">The context to configure.</param>
        /// <param name="url">The URL to set for this context's request.</param>
        /// <returns>A <see cref="HttpContext" /> configured to match the specified API context and URI.</returns>
        public static HttpContext ConfigureHttp(this ApiOperationContext context, string url)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
            httpContext.SetRequestUri(url);
            httpContext.Request.Method = context.Descriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod;
            httpContext.Request.Headers["Content-Type"] = "application/test-data";

            context.SetHttpFeatureContext(new HttpFeatureContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
            });

            httpContext.SetBaseUri("https://api.blueprint-testing.com/api/");

            httpContext.RequestServices = context.ServiceProvider;

            context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;

            return httpContext;
        }
    }
}
