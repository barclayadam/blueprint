using System;
using System.Collections.Generic;
using System.Net;
using Blueprint.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tests.Api
{
    public static class ApiOperationContextSetup
    {
        public static ApiOperationContext CreateFromDescriptor(IServiceProvider serviceProvider, ApiOperationDescriptor descriptor)
        {
            var dataModel = new ApiDataModel();

            dataModel.RegisterOperation(descriptor);

            var context = new ApiOperationContext(serviceProvider, dataModel, descriptor);

            SetUpTestRequest(context, "https://blueprintapi.com/" + descriptor.Name);

            return context;
        }

        private static void SetUpTestRequest(ApiOperationContext context, string url)
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
