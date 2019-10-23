using Blueprint.Api.Authorisation;
using Blueprint.Api.Middleware;
using Blueprint.ApplicationInsights;
using Blueprint.Sample.WebApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Sample.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWeatherDataSource, WeatherDataSource>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClaimsIdentityProvider, HttpRequestClaimsIdentityProvider>();

            services.AddControllers();

            services.AddBlueprintApi(o =>
            {
                o.WithApplicationName("SampleWebApi");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ApplicationInsightsMiddleware>();
                o.UseMiddlewareBuilder<HttpMessagePopulationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ValidationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<AuthenticationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<UserContextLoaderMiddlewareBuilder>();
                o.UseMiddlewareBuilder<AuthorisationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<LinkGeneratorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ResourceEventHandlerMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.Scan(typeof(Startup).Assembly);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseBlueprintApi("api/");
        }
    }
}
