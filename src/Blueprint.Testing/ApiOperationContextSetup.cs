using System;
using System.Collections.Generic;
using System.Net;
using Blueprint.Api;
using Microsoft.AspNetCore.Http;
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
        /// Constructs a new <see cref="ApiOperationContext" /> from a configured <see cref="IServiceProvider" /> and
        /// <see cref="ApiOperationDescriptor" />.
        /// </summary>
        /// <param name="serviceProvider">The service provider to create a context with.</param>
        /// <param name="descriptor">The descriptor of the operation this context will be for.</param>
        /// <returns>A newly configured <see cref="ApiOperationContext"/>.</returns>
        public static ApiOperationContext CreateFromDescriptor(IServiceProvider serviceProvider, ApiOperationDescriptor descriptor)
        {
            var dataModel = new ApiDataModel();

            dataModel.RegisterOperation(descriptor);

            var context = new ApiOperationContext(serviceProvider, dataModel, descriptor);

            SetUpTestRequest(context, "https://api.blueprint.com/" + descriptor.Name);

            return context;
        }

        /// <summary>
        /// Configures the <see cref="ApiOperationContext" /> with a HTTP context configured for the given URL.
        /// </summary>
        /// <remarks>
        /// In addition to setting <see cref="ApiOperationContext.HttpContext" /> (and therefore also <see cref="ApiOperationContext.Request"/> and
        /// <see cref="ApiOperationContext.Response"/>) this method will set the newly created <see cref="HttpContext" /> on the
        /// <see cref="HttpContextAccessor" /> that has been registered with the context's <see cref="IServiceProvider"/>.
        /// </remarks>
        /// <param name="context">The context to configure.</param>
        /// <param name="url">The URL to set for this context's request.</param>
        public static void SetUpTestRequest(this ApiOperationContext context, string url)
        {
            var uri = new Uri(url);

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Path = new PathString(uri.LocalPath);
            httpContext.Request.QueryString = new QueryString(uri.Query);
            httpContext.Request.Method = context.Descriptor.HttpMethod.ToString();

            context.HttpContext = httpContext;
            context.RouteData = new Dictionary<string, object>();

            context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;
        }
    }
}
