using System;
using Blueprint.Apm;
using Blueprint.Apm.ApplicationInsights;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Configuration;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder{THost}" /> to add Application Insights integration.
    /// </summary>
    public static class BlueprintBuilderExtensions
    {
        /// <summary>
        /// Adds Application Insights integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies and operations, and middleware that will set properties of the current
        /// <see cref="RequestTelemetry" /> of the current HTTP context (if it exists).
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <typeparam name="THost">The type of host.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        public static BlueprintApiBuilder<THost> AddApplicationInsights<THost>(this BlueprintApiBuilder<THost> pipelineBuilder)
        {
            pipelineBuilder.Services.AddSingleton<IApmTool, ApplicationInsightsApmTool>();

            pipelineBuilder.Compilation(c => c.AddVariableSource(new HttpRequestTelemetrySource()));

            return pipelineBuilder;
        }

        private class HttpRequestTelemetrySource : IVariableSource
        {
            public Variable TryFindVariable(IMethodVariables variables, Type type)
            {
                if (type == typeof(RequestTelemetry))
                {
                    // We special-case if we find a HttpContext variable to grab the RequestTelemetry from
                    // the Features collection. Otherwise we do nothing here and rely on dependency injection
                    // to find the variable
                    var httpContextVariable = variables.TryFindVariable(typeof(HttpContext));

                    if (httpContextVariable != null)
                    {
                        var features = httpContextVariable.GetProperty(nameof(HttpContext.Features));
                        var methodCall = MethodCall.For<IFeatureCollection>(c => c.Get<RequestTelemetry>());
                        methodCall.Target = features;

                        return methodCall.ReturnVariable;
                    }
                }

                return null;
            }
        }
    }
}
