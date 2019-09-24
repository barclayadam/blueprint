using System;
using Blueprint.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace Blueprint.Sample.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvcCore();
            services.AddBlueprintApi(o =>
            {
                o.WithApplicationName("SampleWebApi");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<MessagePopulationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ValidationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
//                o.UseMiddlewareBuilder<BackgroundTaskRunnerMiddleware>();
                o.UseMiddlewareBuilder<ResourceEventHandlerMiddlewareBuilder>();
                o.UseMiddlewareBuilder<LinkGeneratorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.ScanForOperations(typeof(Startup).Assembly);
            });

            var container = new Container();

            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseBlueprintApi("api/");
        }
    }
}
