using System;
using System.Collections.Generic;
using System.Net;
using Blueprint.Api;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Tests.Api
{
    public static class ApiOperationContextSetup
    {
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

            context.Container.Inject(typeof(IHttpContextAccessor), new HttpContextAccessor { HttpContext = httpContext });
        }

        public static ApiOperationContext CreateFromDescriptor(IServiceProvider serviceProvider, ApiOperationDescriptor descriptor)
        {
            var dataModel = new ApiDataModel();

            dataModel.RegisterOperation(descriptor);

            var context = new ApiOperationContext(serviceProvider, dataModel, descriptor);

            context.SetUpTestRequest("https://blueprintapi.com/" + descriptor.Name);

            return context;
        }
    }
}
