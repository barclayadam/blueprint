using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Sample.WebApi.Data;
using Blueprint.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Blueprint.Sample.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IWeatherDataSource, WeatherDataSource>();

            services.AddHangfire(h =>
            {
                h
                    .UseStorage(new SqlServerStorage("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog=blueprint-examples"))
                    .UseRecommendedSerializerSettings();
            });

            services.AddOpenTelemetryTracing(
                builder => builder
                    .SetResourceBuilder(ResourceBuilder.CreateEmpty().AddService("web-api"))
                    .AddAspNetCoreInstrumentation()
                    .AddBlueprintInstrumentation()
                    .AddJaegerExporter()
                    .AddConsoleExporter()
            );

            services.AddBlueprintApi(b => b
                .Http()
                .SetApplicationName("SampleWebApi")
                .Operations(o => o.Scan(typeof(WebApi.Startup).Assembly))
                .AddTasksClient(t => t.UseHangfire())
                .AddOpenApi()
                .AddLogging()
                .AddValidation()
                .AddHateoasLinks()
                .AddResourceEvents<NullResourceEventRepository>());
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
                endpoints.MapBlueprintApi("api/")
                    .RequireHost("localhost:49454");
            });
        }
    }
}
