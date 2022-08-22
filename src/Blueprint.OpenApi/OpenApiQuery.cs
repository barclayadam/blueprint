using Blueprint.Authorisation;
using Blueprint.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSwag;

namespace Blueprint.OpenApi;

/// <summary>
/// An <see cref="IQuery{PlainTextResult}" /> that can will return an OpenAPI representation of the
/// <see cref="ApiDataModel" /> of the current API.
/// </summary>
[AllowAnonymous]
[RootLink("/openapi")]
[UnexposedOperation]
public class OpenApiQuery : IQuery<PlainTextResult>
{
    /// <summary>
    /// Returns the OpenAPI representation of the given <see cref="ApiDataModel" />.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext" />.</param>
    /// <param name="openApiDocument">The <see cref="OpenApiDocument" />, as built by the <see cref="OpenApiDocumentBuilder" />.</param>
    /// <param name="context">The API operation context.</param>
    /// <param name="options">The options to configure the OpenAPI document.</param>
    /// <returns>An OpenAPI representation.</returns>
    public PlainTextResult Invoke(
        HttpContext httpContext,
        OpenApiDocument openApiDocument,
        ApiOperationContext context,
        IOptions<OpenApiOptions> options)
    {
        var openApiOptions = options.Value;

        // If the document does not have any servers defined explicitly by client application we will push
        // a default one that is the currently running server
        if (openApiDocument.Servers.Count == 0)
        {
            var baseUri = httpContext.GetBlueprintBaseUri();

            openApiDocument.Servers.Add(new OpenApiServer
            {
                Url = baseUri,
            });
        }

        var httpRequest = httpContext.Request;

        // If we believe this is a hit from a browser then serve up the documentation using Refit. This can be overriden
        // by passing a json query string to force the JSON response.
        if (httpRequest.Headers["Accept"].ToString().Contains("text/html") && httpRequest.Query.ContainsKey("json") == false)
        {
            var baseUri = httpContext.GetBlueprintBaseUri();
            var refitHtmlDocument = @$"<!DOCTYPE html>
<html>
<head>
    <title>{openApiDocument.Info.Title}</title>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:300,400,700|Roboto:300,400,700"" rel=""stylesheet"">

    <style>
        body {{
            margin: 0;
            padding: 0;
        }}
    </style>
</head>
<body>
<redoc spec-url='{baseUri.TrimEnd('/')}/openapi'></redoc>
<script src=""https://cdn.jsdelivr.net/npm/redoc@{openApiOptions.RedocVersion}/bundles/redoc.standalone.js""></script>
</body>
</html>";

            return new PlainTextResult(refitHtmlDocument)
            {
                ContentType = "text/html",
            };
        }

        // We use the feature data to cache the generated JSON and PlainTextResult, which can shave off ms on
        // subsequent calls and potentially reduce memory by _a lot_
        if (!context.Descriptor.TryGetFeatureData(out OpenApiJsonDocumentFeature openApiJson))
        {
            openApiJson = new OpenApiJsonDocumentFeature(new PlainTextResult(openApiDocument.ToJson(openApiOptions.SchemaType, openApiOptions.Formatting))
            {
                ContentType = "application/json",
            });

            context.Descriptor.SetFeatureData(openApiJson);
        }

        return openApiJson.JsonResult;
    }

    private class OpenApiJsonDocumentFeature
    {
        public OpenApiJsonDocumentFeature(PlainTextResult jsonResult)
        {
            this.JsonResult = jsonResult;
        }

        public PlainTextResult JsonResult { get; }
    }
}