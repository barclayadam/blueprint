using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

using NLog;

using StructureMap;

namespace Blueprint.Core.Api
{
    public static class ApplicationBuilderApiExtensions
    {
        public static void UseBlueprintApi(
            this IApplicationBuilder applicationBuilder,
            string apiPrefix,
            BlueprintApiOptions options)
        {
            var container = (IContainer)applicationBuilder.ApplicationServices.GetService(typeof(IContainer));
            var apiModel = options.Model;

            // Ensure ends with a slash, but only one
            apiPrefix = apiPrefix.TrimEnd('/') + '/';

            var apiExecutorBuilder = new ApiOperationExecutorBuilder();
            var apiExecutor = apiExecutorBuilder.Build(options, container);

            var routeBuilder = new RouteBuilder(applicationBuilder);
            var routeHandler = new BlueprintApiRouter(apiExecutor, container);
            var inlineConstraintResolver =
                applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();

            // Ordering by 'indexOf {' means we put those URLs which are not placeholders
            // first (e.g. /users/{id} and /users/me will put /users/me first)
            foreach (var link in apiModel.Links.OrderBy(l => l.UrlFormat.IndexOf('{')))
            {
                var safeRouteUrl = link.GetFormatForRouting();

                routeBuilder.Routes.Add(new Route(
                    target: routeHandler,
                    routeName: link.UrlFormat + "-" + link.OperationDescriptor.HttpMethod,
                    routeTemplate: apiPrefix + safeRouteUrl,
                    defaults: new RouteValueDictionary(new
                    {
                        operation = link.OperationDescriptor
                    }),
                    constraints: new Dictionary<string, object>
                    {
                        ["httpMethod"] = new HttpMethodRouteConstraint(link.OperationDescriptor.HttpMethod.ToString())
                    },
                    dataTokens: null,
                    inlineConstraintResolver: inlineConstraintResolver));
            }

            if (options.NotFoundMode == NotFoundMode.Handle)
            {
                routeBuilder.Routes.Add(new Route(
                    target: new BlueprintApiNotFoundRouter(),
                    routeName: "api-not-found",
                    routeTemplate: apiPrefix + "{*url}",
                    defaults: new RouteValueDictionary(),
                    constraints: null,
                    dataTokens: null,
                    inlineConstraintResolver: inlineConstraintResolver));
            }

            applicationBuilder.UseRouter(routeBuilder.Build());
        }

        private class BlueprintApiRouter : IRouter
        {
            private static readonly Logger Log = LogManager.GetCurrentClassLogger();

            private readonly IApiOperationExecutor apiOperationExecutor;
            private readonly IContainer rootContainer;

            public BlueprintApiRouter(
                IApiOperationExecutor apiOperationExecutor,
                IContainer rootContainer)
            {
                this.apiOperationExecutor = apiOperationExecutor;
                this.rootContainer = rootContainer;
            }

            public Task RouteAsync(RouteContext context)
            {
                context.Handler = async c =>
                {
                    var operation = (ApiOperationDescriptor) context.RouteData.Values["operation"];

                    if (operation.HttpMethod.ToString() != context.HttpContext.Request.Method)
                    {
                        Log.Info(
                            "Request does not match required HTTP method. url={0} request_method={1} operation_method={2}",
                            context.HttpContext.Request.GetDisplayUrl(),
                            context.HttpContext.Request.Method,
                            operation.HttpMethod);

                        context.HttpContext.Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;

                        return;
                    }

                    using (var nestedContainer = rootContainer.GetNestedContainer())
                    {
                        var apiContext = new ApiOperationContext(nestedContainer, apiOperationExecutor.DataModel, operation)
                        {
                            RouteData = context.RouteData.Values,
                            HttpContext = context.HttpContext
                        };

                        var result = await apiOperationExecutor.Execute(apiContext);

                        // We want to immediately execute the result to allow it to write to the HTTP response
                        result.Execute(apiContext);
                    }
                };

                return Task.CompletedTask;
            }

            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class BlueprintApiNotFoundRouter : IRouter
        {
            private static readonly Logger Log = LogManager.GetCurrentClassLogger();

            public Task RouteAsync(RouteContext context)
            {
                context.Handler = c =>
                {
                    Log.Info(
                        "Request does not match API endpoint. url={0}",
                        context.HttpContext.Request.GetDisplayUrl());

                    context.HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;

                    return Task.CompletedTask;
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
