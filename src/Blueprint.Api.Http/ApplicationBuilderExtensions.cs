using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Http;
using Blueprint.Compiler;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.CodeAnalysis;
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
            // Ensure ends with a slash, but only one
            apiPrefix = apiPrefix.TrimEnd('/') + '/';

            var routeBuilder = new RouteBuilder(applicationBuilder);
            var apiDataModel = applicationBuilder.ApplicationServices.GetRequiredService<ApiDataModel>();
            var inlineConstraintResolver = applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();

            IRouter routeHandler;

            try
            {
                var apiOperationExecutor = applicationBuilder.ApplicationServices.GetRequiredService<IApiOperationExecutor>();

                routeHandler = new BlueprintApiRouter(
                    apiOperationExecutor,
                    applicationBuilder.ApplicationServices,
                    applicationBuilder.ApplicationServices.GetRequiredService<ILogger<BlueprintApiRouter>>());
            }
            catch (CompilationException e)
            {
                // When a compilation exception occurs (happens when getting IApiOperationExecutor for the first time as it is
                // registered as a singleton factory) we create a route handler that will throw a WrapperException.
                //
                // The WrapperException implements ICompilationException which means if we are using the developer exception
                // page (i.e. app.UseDeveloperExceptionPage()) we get a nice compilation error page that includes the error
                // message AND complete file source code to enable much better diagnostics of the compilation issue).
                //
                // The consequence of this is that the app is NOT prevented from starting up and only when hitting an API endpoint
                // would be exception be known to HTTP clients (the error is logged regardless)
                routeHandler = new RouteHandler(context => throw new CompilationWrapperException(e));
            }
            catch (Exception e) when (e.InnerException is CompilationException ce)
            {
                // Some DI projects wrap the CompilationException in their own (i.e. StructureMap, which throws a specific exception
                // when trying to create an instance). This catch block attempts to handle those custom exceptions on the assumption that
                // the inner exception would be the original CompilationException
                routeHandler = new RouteHandler(context => throw new CompilationWrapperException(ce));
            }

            // Ordering by 'indexOf {' means we put those URLs which are not placeholders
            // first (e.g. /users/{id} and /users/me will put /users/me first)
            foreach (var link in apiDataModel.Links.OrderBy(l => l.UrlFormat.IndexOf('{')))
            {
                routeBuilder.Routes.Add(new Route(
                    target: routeHandler,
                    routeName: link.UrlFormat + "-" + link.OperationDescriptor.HttpMethod,
                    routeTemplate: apiPrefix + link.RoutingUrl,
                    defaults: new RouteValueDictionary(new {operation = link.OperationDescriptor}),
                    constraints: new Dictionary<string, object>
                    {
                        ["httpMethod"] = new HttpMethodRouteConstraint(link.OperationDescriptor.HttpMethod.ToString()),
                    },
                    dataTokens: null,
                    inlineConstraintResolver: inlineConstraintResolver));
            }

            applicationBuilder.UseRouter(routeBuilder.Build());
        }

        private class CompilationWrapperException : Exception, ICompilationException
        {
            private readonly CompilationException compilationException;

            public CompilationWrapperException(CompilationException compilationException) : base("Pipeline compilation failed", compilationException)
            {
                this.compilationException = compilationException;
            }

            public IEnumerable<CompilationFailure> CompilationFailures => compilationException.Failures
                .Select(f =>
                {
                    var sourceTree = f.Location.SourceTree;
                    var sourceCode = sourceTree.ToString();

                    return new CompilationFailure(
                        sourceTree.FilePath,
                        sourceCode,
                        sourceCode,
                        new[] {ToDiagnosticMessage(f)});
                });

            private static DiagnosticMessage ToDiagnosticMessage(Diagnostic f)
            {
                var span = f.Location.GetLineSpan();
                var mappedSpan = f.Location.GetMappedLineSpan();
                var message = f.GetMessage();

                return new DiagnosticMessage(
                    message,
                    message,
                    f.Location.SourceTree.FilePath,
                    mappedSpan.Span.Start.Line,
                    span.StartLinePosition.Character,
                    mappedSpan.Span.End.Line,
                    span.EndLinePosition.Character);
            }
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
                        var apiContext = new ApiOperationContext(nestedContainer.ServiceProvider, apiOperationExecutor.DataModel, operation);

                        apiContext.SetRouteContext(context);

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
