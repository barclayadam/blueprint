using System;
using Blueprint.Core;

namespace Blueprint.Api.Configuration
{
    public class BlueprintMiddlewareConfigurer
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;

        public BlueprintMiddlewareConfigurer(BlueprintApiConfigurer blueprintApiConfigurer)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
        }

        public BlueprintMiddlewareConfigurer Logging(Action<BlueprintLoggingConfigurer> configurer, MiddlewareStage? middlewareStage = null)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintLoggingConfigurer(blueprintApiConfigurer, middlewareStage));

            return this;
        }

        public BlueprintMiddlewareConfigurer Validation(Action<BlueprintValidationConfigurer> configurer, MiddlewareStage? middlewareStage = null)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintValidationConfigurer(blueprintApiConfigurer, middlewareStage));

            return this;
        }

        public BlueprintMiddlewareConfigurer Auditing(Action<BlueprintAuditConfigurer> configurer, MiddlewareStage? middlewareStage = null)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintAuditConfigurer(blueprintApiConfigurer, middlewareStage));

            return this;
        }
    }
}
