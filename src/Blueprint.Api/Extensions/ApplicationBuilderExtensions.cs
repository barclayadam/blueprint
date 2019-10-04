using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// This is the recommendation from MS for extensions to IApplicationBuilder to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseBlueprintApi(
            this IApplicationBuilder applicationBuilder,
            string apiPrefix)
        {
            var options = applicationBuilder.ApplicationServices.GetRequiredService<BlueprintApiOptions>();
            var apiDataModel = applicationBuilder.ApplicationServices.GetRequiredService<ApiDataModel>();
            var apiOperationExecutor = applicationBuilder.ApplicationServices.GetRequiredService<IApiOperationExecutor>();
            var inlineConstraintResolver = applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();

            // Ensure ends with a slash, but only one
            apiPrefix = apiPrefix.TrimEnd('/') + '/';

            var routeBuilder = new RouteBuilder(applicationBuilder);
            var routeHandler = new BlueprintApiRouter(
                apiOperationExecutor,
                applicationBuilder.ApplicationServices,
                applicationBuilder.ApplicationServices.GetRequiredService<ILogger<BlueprintApiRouter>>());

            // Ordering by 'indexOf {' means we put those URLs which are not placeholders
            // first (e.g. /users/{id} and /users/me will put /users/me first)
            foreach (var link in apiDataModel.Links.OrderBy(l => l.UrlFormat.IndexOf('{')))
            {
                var safeRouteUrl = link.GetFormatForRouting();

                routeBuilder.Routes.Add(new Route(
                    target: routeHandler,
                    routeName: link.UrlFormat + "-" + link.OperationDescriptor.HttpMethod,
                    routeTemplate: apiPrefix + safeRouteUrl,
                    defaults: new RouteValueDictionary(new
                    {
                        operation = link.OperationDescriptor,
                    }),
                    constraints: new Dictionary<string, object>
                    {
                        ["httpMethod"] = new HttpMethodRouteConstraint(link.OperationDescriptor.HttpMethod.ToString()),
                    },
                    dataTokens: null,
                    inlineConstraintResolver: inlineConstraintResolver));
            }

            applicationBuilder.UseRouter(routeBuilder.Build());
        }

        private class BlueprintApiRouter : IRouter
        {
            private readonly IApiOperationExecutor apiOperationExecutor;
            private readonly IServiceProvider rootServiceProvider;
            private readonly ILogger<BlueprintApiRouter> logger;

            public BlueprintApiRouter(
                IApiOperationExecutor apiOperationExecutor,
                IServiceProvider rootServiceProvider,
                ILogger<BlueprintApiRouter> logger)
            {
                this.apiOperationExecutor = apiOperationExecutor;
                this.rootServiceProvider = rootServiceProvider;
                this.logger = logger;
            }

            public Task RouteAsync(RouteContext context)
            {
                context.Handler = async c =>
                {
                    var operation = (ApiOperationDescriptor)context.RouteData.Values["operation"];

                    if (operation.HttpMethod.ToString() != context.HttpContext.Request.Method)
                    {
                        logger.LogInformation(
                            "Request does not match required HTTP method. url={0} request_method={1} operation_method={2}",
                            context.HttpContext.Request.GetDisplayUrl(),
                            context.HttpContext.Request.Method,
                            operation.HttpMethod);

                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                        return;
                    }

                    using (var nestedContainer = rootServiceProvider.CreateScope())
                    {
                        var apiContext = new ApiOperationContext(nestedContainer.ServiceProvider, apiOperationExecutor.DataModel, operation)
                        {
                            RouteData = context.RouteData.Values,
                            HttpContext = context.HttpContext,
                        };

                        var result = await apiOperationExecutor.ExecuteAsync(apiContext);

                        // We want to immediately execute the result to allow it to write to the HTTP response
                        await result.ExecuteAsync(apiContext);
                    }
                };

                return Task.CompletedTask;
            }

            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
