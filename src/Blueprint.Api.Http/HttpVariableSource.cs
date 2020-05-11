using System;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An <see cref="IVariableSource" /> that can creates variables for HTTP-related values for an
    /// operation pipeline.
    /// </summary>
    /// <remarks>
    /// The given properties can be matched:
    ///
    ///  * <see cref="HttpFeatureContext" />
    ///  * <see cref="HttpContext" />
    ///  * <see cref="HttpRequest" />
    ///  * <see cref="HttpResponse" />
    ///  * <see cref="RouteData" />
    /// </remarks>
    public class HttpVariableSource : IVariableSource
    {
        /// <inheritdoc />
        public Variable TryFindVariable(IMethodVariables variables, Type type)
        {
            if (type == typeof(HttpFeatureContext))
            {
                var httpFeatureContextVariable = new MethodCall(
                    typeof(ApiOperationContextHttpExtensions),
                    nameof(ApiOperationContextHttpExtensions.GetHttpFeatureContext));

                return httpFeatureContextVariable.ReturnVariable;
            }

            if (type == typeof(HttpContext))
            {
                var httpContextVariable = new MethodCall(
                    typeof(ApiOperationContextHttpExtensions),
                    nameof(ApiOperationContextHttpExtensions.GetHttpContext));

                return httpContextVariable.ReturnVariable;
            }

            // Note that we _do not_ re-use the definition of HttpContext from above. We instead grab it from variables
            // so that the call can be properly registered (otherwise the output would be httpContext.Request but WITHOUT
            // httpContext having been defined
            if (type == typeof(HttpRequest))
            {
                return variables.FindVariable(typeof(HttpContext)).GetProperty(nameof(HttpContext.Request));
            }

            if (type == typeof(HttpResponse))
            {
                return variables.FindVariable(typeof(HttpContext)).GetProperty(nameof(HttpContext.Response));
            }

            if (type == typeof(RouteData))
            {
                var routeDataVariable = new MethodCall(typeof(ApiOperationContextHttpExtensions), nameof(ApiOperationContextHttpExtensions.GetRouteData));

                return routeDataVariable.ReturnVariable;
            }

            return null;
        }
    }
}
