using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint;
using Blueprint.Compiler;
using Blueprint.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// This is the recommendation from MS for extensions to IEndpointRouteBuilder to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all Blueprint operation endpoints that have previously been registered with the dependency
    /// injection host using <see cref="ServiceCollectionExtensions.AddBlueprintApi" />.
    /// </summary>
    /// <remarks>
    /// Note that if the compilation of the pipelines fail this method does <b>not</b> throw that exception
    /// but instead will register handlers that expose the compilation exception, that when combined with
    /// the development exception page can present useful diagnostics for tracking down the compilation
    /// error.
    /// </remarks>
    /// <param name="endpoints">The endpoint builder to register with.</param>
    /// <param name="basePath">A base path to prepend to all routes.</param>
    /// <returns>A builder that allows adding metadata / conventions to all mapped endpoints.</returns>
    public static IEndpointConventionBuilder MapBlueprintApi(
        this IEndpointRouteBuilder endpoints,
        string basePath)
    {
        // Ensure ends with a slash, but only one
        basePath = basePath.TrimEnd('/') + '/';

        var logger = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Blueprint.Compilation");
        var apiDataModel = endpoints.ServiceProvider.GetRequiredService<ApiDataModel>();
        var httpOptions = endpoints.ServiceProvider.GetRequiredService<IOptions<BlueprintHttpOptions>>();

        RequestDelegate requestDelegate;

        try
        {
            var apiOperationExecutor = endpoints.ServiceProvider.GetRequiredService<IApiOperationExecutor>();

            requestDelegate = new BlueprintApiRouter(
                apiOperationExecutor,
                endpoints.ServiceProvider,
                endpoints.ServiceProvider.GetRequiredService<ILogger<BlueprintApiRouter>>(),
                httpOptions,
                basePath).RouteAsync;
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
            requestDelegate = _ => throw new CompilationWrapperException(e);
        }
        catch (Exception e) when (e.InnerException is CompilationException ce)
        {
            logger.LogCritical(e.InnerException, "Blueprint pipeline compilation failed");

            // Some DI projects wrap the CompilationException in their own (i.e. StructureMap, which throws a specific exception
            // when trying to create an instance). This catch block attempts to handle those custom exceptions on the assumption that
            // the inner exception would be the original CompilationException
            requestDelegate = _ => throw new CompilationWrapperException(ce);
        }

        var builders = new List<IEndpointConventionBuilder>();

        var allLinks = new List<ApiOperationLink>();

        foreach (var descriptor in apiDataModel.Operations)
        {
            if (!descriptor.TryGetFeatureData<HttpOperationFeatureData>(out var httpOperationFeatureData))
            {
                continue;
            }

            allLinks.AddRange(httpOperationFeatureData.Links);
        }

        // Ordering by 'indexOf {' means we put those URLs which are not placeholders
        // first (e.g. /users/{id} and /users/me will put /users/me first) because those without would return -1
        foreach (var link in allLinks.OrderBy(l => l.UrlFormat.IndexOf('{')))
        {
            var httpFeatureData = link.OperationDescriptor.GetFeatureData<HttpOperationFeatureData>();

            var builder = endpoints.Map(RoutePatternFactory.Parse(basePath + link.RoutingUrl), requestDelegate);

            builder.WithDisplayName($"{httpFeatureData.HttpMethod} {link.OperationDescriptor.Name}");
            builder.WithMetadata(new HttpMethodMetadata(new[] { httpFeatureData.HttpMethod }));
            builder.WithMetadata(link.OperationDescriptor);

            builders.Add(builder);
        }

        return new BlueprintEndpointRouteBuilder(builders);
    }

    /// <summary>
    /// A wrapper around the core <see cref="CompilationException" /> that integrates it with the
    /// ASP.Net <see cref="ICompilationException" /> feature to provide a nicer view of the compilation issues
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

    private class BlueprintApiRouter
    {
        private readonly IApiOperationExecutor _apiOperationExecutor;
        private readonly IServiceProvider _rootServiceProvider;
        private readonly ILogger<BlueprintApiRouter> _logger;
        private readonly IOptions<BlueprintHttpOptions> _httpOptions;
        private readonly string _basePath;

        public BlueprintApiRouter(
            IApiOperationExecutor apiOperationExecutor,
            IServiceProvider rootServiceProvider,
            ILogger<BlueprintApiRouter> logger,
            IOptions<BlueprintHttpOptions> httpOptions,
            string basePath)
        {
            this._apiOperationExecutor = apiOperationExecutor;
            this._rootServiceProvider = rootServiceProvider;
            this._logger = logger;
            this._httpOptions = httpOptions;
            this._basePath = basePath;
        }

        public async Task RouteAsync(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var routeData = httpContext.GetRouteData();
            var httpRequest = httpContext.Request;

            var operation = endpoint.Metadata.GetMetadata<ApiOperationDescriptor>();

            // If we have an activity set the DisplayName to the operation type
            if (Activity.Current != null)
            {
                Activity.Current.DisplayName = operation.Name;
            }

            var httpFeatureData = operation.GetFeatureData<HttpOperationFeatureData>();

            if (httpFeatureData.HttpMethod != httpRequest.Method)
            {
                this._logger.LogInformation(
                    "Request {Method} {Url} does not match required HTTP method {RequiredMethod}",
                    httpRequest.Method,
                    httpRequest.GetDisplayUrl(),
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
                ClaimsIdentity = httpContext.User.Identity as ClaimsIdentity,
            };

            apiContext.SetHttpFeatureContext(new HttpFeatureContext
            {
                HttpContext = httpContext,
                RouteData = routeData,
            });

            var request = httpContext.Request;
            var baseUri = $"{request.Scheme}://{this._httpOptions.Value.PublicHost ?? request.Host.Value}{request.PathBase}/{this._basePath}";

            httpContext.SetBaseUri(baseUri);

            var result = await this._apiOperationExecutor.ExecuteAsync(apiContext);

            // We want to immediately execute the result to allow it to write to the HTTP response
            await result.ExecuteAsync(apiContext);
        }
    }
}
