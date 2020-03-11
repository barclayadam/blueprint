using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Sample.WebApi.Data;
using Blueprint.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Sample.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IWeatherDataSource, WeatherDataSource>();

            services.AddApplicationInsightsTelemetry();

            services.AddHangfire(h =>
            {
                h
                    .UseStorage(new SqlServerStorage("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog=blueprint-examples"));
            });

            services.AddBlueprintApi(b => b
                .SetApplicationName("SampleWebApi")
                .Operations(o => o.ScanForOperations(typeof(Startup).Assembly))
                .AddHttp()
                .AddTasksClient(t => t.UseHangfire())
                .AddApplicationInsights()
                .Pipeline(m => m
                    .AddLogging()
                    .AddValidation()
                    .AddHateoasLinks()
                    .AddResourceEvents<NullResourceEventRepository>()
                ));
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
