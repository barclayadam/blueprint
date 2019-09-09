using System;
using Blueprint.Api;
using Blueprint.Api.Validation;
using Blueprint.Core.Auditing;
using Blueprint.SqlServer;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blueprint.ServiceProvider
{
    public static class HostBuilderExtensions
    {
        //public static readonly Type[] DefaultMiddlewareToRegister =
        //{
        //    typeof(LoggingMiddlewareBuilder),
        //    typeof(ErrorHandlerMiddlewareBuilder),
        //    typeof(AuthenticationMiddlewareBuilder),
        //    typeof(AuditMiddlewareBuilder),
        //    typeof(ValidationMiddlewareBuilder),
        //    typeof(AuthorisationMiddlewareBuilder)
        //};

        public static IHostBuilder UseBlueprintApi(
            this IHostBuilder hostBuilder,
            string appName,
            Action<BlueprintApiOptions> options)
        {
            hostBuilder.ConfigureContainer((HostBuilderContext hostBuilderContext, ServiceRegistry services) =>
            {
                var container = new Container(services);

                //options = new BlueprintApiOptions(o =>
                //{
                //    o.WithApplicationName(appName);

                //    o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                //    o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                //    o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();
                //});

                //if (middlewares == DefaultMiddlewareToRegister)
                //{
                    //services.AddTransient<IUserAuthorisationContextFactory, DefaultUserAuthorisationContextFactory>();
                    services.AddSingleton<IAuditor, SqlServerAuditor>();
                    services.AddTransient<IValidator, BlueprintValidator>();
                //}

                var codeGennedExecutor = CreateExecutor(container, options);

                var api = new Api.Api(codeGennedExecutor);

                services.AddSingleton<IApi>(api);
            });

            return hostBuilder;
        }

        private static CodeGennedExecutor CreateExecutor(IContainer container, Action<BlueprintApiOptions> options)
        {
            //return new ApiOperationExecutorBuilder().Build(new BlueprintApiOptions(options), container);
            return new CodeGennedExecutor(null, null, null);
        }
    }
}
