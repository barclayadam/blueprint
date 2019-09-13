using System;
using Blueprint.Api.Middleware;
using Blueprint.Core;

namespace Blueprint.Api.Configuration
{
    public class BlueprintMiddlewareConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;

        public BlueprintMiddlewareConfigurer(BlueprintConfigurer blueprintConfigurer)
        {
            this.blueprintConfigurer = blueprintConfigurer;
        }

        public BlueprintMiddlewareConfigurer Logging(MiddlewareStage? middlewareStage = null)
        {
            blueprintConfigurer.AddMiddlewareBuilderToStage<LoggingMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.OperationChecks);

            return this;
        }

        public BlueprintMiddlewareConfigurer Validation(Action<BlueprintValidationConfigurer> configurer, MiddlewareStage? middlewareStage = null)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintValidationConfigurer(blueprintConfigurer, middlewareStage));

            return this;
        }

        public BlueprintMiddlewareConfigurer Auditing(Action<BlueprintAuditConfigurer> configurer, MiddlewareStage? middlewareStage = null)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintAuditConfigurer(blueprintConfigurer, middlewareStage));

            return this;
        }
    }
}
