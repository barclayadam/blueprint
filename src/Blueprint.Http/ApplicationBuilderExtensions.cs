using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint;
using Blueprint.Apm;
using Blueprint.Compiler;
using Blueprint.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// This is the recommendation from MS for extensions to IApplicationBuilder to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseBlueprintApi(
            this IApplicationBuilder applicationBuilder,
            string basePath)
        {
            // Ensure ends with a slash, but only one
            basePath = basePath.TrimEnd('/') + '/';

            var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Blueprint.Compilation");
            var routeBuilder = new RouteBuilder(applicationBuilder);
            var apiDataModel = applicationBuilder.ApplicationServices.GetRequiredService<ApiDataModel>();
            var inlineConstraintResolver = applicationBuilder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>();
            var apmTool = applicationBuilder.ApplicationServices.GetRequiredService<IApmTool>();
            var httpOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<BlueprintHttpOptions>>();

            IRouter routeHandler;

            try
            {
                var apiOperationExecutor = applicationBuilder.ApplicationServices.GetRequiredService<IApiOperationExecutor>();

                routeHandler = new BlueprintApiRouter(
                    apiOperationExecutor,
                    apmTool,
                    applicationBuilder.ApplicationServices,
                    applicationBuilder.ApplicationServices.GetRequiredService<ILogger<BlueprintApiRouter>>(),
                    httpOptions,
                    basePath);
            }
            catch (CompilationException e)
            {
                logger.LogCritical(e, "Blueprint pipeline compilation failed");

                // When a compilation exception occurs (happens when getting IApiOperationExecutor for the first time as it is
                // registered as a singleton factory) we create a route handler that will throw a WrapperException.
                //
                // The WrapperException implements ICompilationException which means if we are using the developer exception
                // page (i.e. app.UseDeveloperExceptionPage()) we get a nice compilation error page that includes the error
                // message AND complete file source code to enable much better diagnostics of the compilation issue).
                //
                // The consequence of this is that the app is NOT prevented from starting up and only when hitting an API endpoint
                // would the exception be known to HTTP clients (the error is logged regardless)
                routeHandler = new RouteHandler(context => throw new CompilationWrapperException(e));
            }
            catch (Exception e) when (e.InnerException is CompilationException ce)
            {
                logger.LogCritical(e.InnerException, "Blueprint pipeline compilation failed");

                // Some DI projects wrap the CompilationException in their own (i.e. StructureMap, which throws a specific exception
                // when trying to create an instance). This catch block attempts to handle those custom exceptions on the assumption that
                // the inner exception would be the original CompilationException
                routeHandler = new RouteHandler(context => throw new CompilationWrapperException(ce));
            }

            // Ordering by 'indexOf {' means we put those URLs which are not placeholders
            // first (e.g. /users/{id} and /users/me will put /users/me first) because those without would return -1
            foreach (var link in apiDataModel.Links.OrderBy(l => l.UrlFormat.IndexOf('{')))
            {
                var httpFeatureData = link.OperationDescriptor.GetFeatureData<HttpOperationFeatureData>();

                routeBuilder.Routes.Add(new Route(
                    target: routeHandler,
                    routeName: httpFeatureData.HttpMethod + "-" + link.UrlFormat,
                    routeTemplate: basePath + link.RoutingUrl,
                    defaults: new RouteValueDictionary(new { operation = link.OperationDescriptor }),
                    constraints: new Dictionary<string, object>
                    {
                        ["httpMethod"] = new HttpMethodRouteConstraint(httpFeatureData.HttpMethod),
                    },
                    dataTokens: null,
                    inlineConstraintResolver: inlineConstraintResolver));
            }

            applicationBuilder.UseRouter(routeBuilder.Build());
        }

        /// <summary>
        /// A wrapper around the core <see cref="CompilationException" /> that integrates it with the
        /// ASP.Net <see cref="ICompilationException" /> to provide a nicer view of the compilation issues
        /// with full source code included.
        /// </summary>
        private class CompilationWrapperException : Exception, ICompilationException
        {
            private readonly CompilationException _compilationException;

            public CompilationWrapperException(CompilationException compilationException)
                : base("Pipeline compilation failed", compilationException)
            {
                this._compilationException = compilationException;
            }

            public IEnumerable<CompilationFailure> CompilationFailures => this._compilationException.Failures
                .Select(f =>
                {
                    var sourceTree = f.Location.SourceTree;
                    var sourceCode = sourceTree.ToString();

                    return new CompilationFailure(
                        sourceTree.FilePath,
                        sourceCode,
                        sourceCode,
                        new[] { ToDiagnosticMessage(f) });
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
            private readonly IApiOperationExecutor _apiOperationExecutor;
            private readonly IApmTool _apmTool;
            private readonly IServiceProvider _rootServiceProvider;
            private readonly ILogger<BlueprintApiRouter> _logger;
            private readonly IOptions<BlueprintHttpOptions> _httpOptions;
            private readonly string _basePath;

            public BlueprintApiRouter(
                IApiOperationExecutor apiOperationExecutor,
                IApmTool apmTool,
                IServiceProvider rootServiceProvider,
                ILogger<BlueprintApiRouter> logger,
                IOptions<BlueprintHttpOptions> httpOptions,
                string basePath)
            {
                this._apiOperationExecutor = apiOperationExecutor;
                this._apmTool = apmTool;
                this._rootServiceProvider = rootServiceProvider;
                this._logger = logger;
                this._httpOptions = httpOptions;
                this._basePath = basePath;
            }

            public Task RouteAsync(RouteContext context)
            {
                context.Handler = async httpContext =>
                {
                    var routeData = httpContext.GetRouteData();
                    var httpRequest = httpContext.Request;

                    var operation = (ApiOperationDescriptor)routeData.Values["operation"];

                    using var apmTransaction = this._apmTool.StartOperation(
                        operation,
                        SpanKinds.Server);

                    apmTransaction.SetTag("span.kind", "server");

                    apmTransaction.SetTag("http.method", httpRequest.Method?.ToUpperInvariant() ?? "UNKNOWN");
                    apmTransaction.SetTag("http.request.headers.host", httpRequest.Host.Value);
                    apmTransaction.SetTag("http.url", httpRequest.GetDisplayUrl());

                    try
                    {
                        var httpFeatureData = operation.GetFeatureData<HttpOperationFeatureData>();

                        if (httpFeatureData.HttpMethod != httpRequest.Method)
                        {
                            this._logger.LogInformation(
                                "Request does not match required HTTP method. url={0} request_method={1} operation_method={2}",
                                httpRequest.GetDisplayUrl(),
                                httpRequest.Method,
                                httpFeatureData.HttpMethod);

                            httpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                            return;
                        }

                        using var nestedContainer = this._rootServiceProvider.CreateScope();

                        var apiContext = new ApiOperationContext(
                            nestedContainer.ServiceProvider,
                            this._apiOperationExecutor.DataModel,
                            operation,
                            httpContext.RequestAborted)
                        {
                            ApmSpan = apmTransaction,
                        };

                        apiContext.SetHttpFeatureContext(new HttpFeatureContext
                        {
                            HttpContext = httpContext,
                            RouteData = routeData,
                        });

                        var request = httpContext.Request;
                        var baseUri = $"{request.Scheme}://{this._httpOptions.Value.PublicHost ?? request.Host.Value}{request.PathBase}/{this._basePath}";

                        httpContext.SetBaseUri(baseUri);

                        apiContext.ClaimsIdentity = context.HttpContext.User.Identity as ClaimsIdentity;

                        var result = await this._apiOperationExecutor.ExecuteAsync(apiContext);

                        // We want to immediately execute the result to allow it to write to the HTTP response
                        await result.ExecuteAsync(apiContext);
                    }
                    catch (Exception e)
                    {
                        // This is NOT an exception from the Pipeline as that is caught and pushed to the transaction
                        // within PushExceptionToApmSpanFrame
                        apmTransaction.RecordException(e);

                        throw;
                    }
                    finally
                    {
                        // We set the specific status code, but rely on the APM integration in the actual pipeline]
                        // to have set the error correctly on the surrounding transaction.
                        apmTransaction.SetTag("http.status_code", httpContext.Response.StatusCode.ToString());
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
