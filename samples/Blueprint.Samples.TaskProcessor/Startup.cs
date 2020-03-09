using Blueprint.Api.Configuration;
using Blueprint.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Samples.TaskProcessor
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.AddBlueprintApi(a => a
                .SetApplicationName("SampleTaskProcessor")
                .Operations(o => o
                    .ScanForOperations(typeof(Startup).Assembly)
                    .ScanForOperations(typeof(Blueprint.Sample.WebApi.Startup).Assembly))
                .AddTasksServer(b => b.UseHangfire(h =>
                {
                    h
                        .UseStorage(new SqlServerStorage("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog=blueprint-examples"))
                        .UseDashboardMetric(SqlServerStorage.ActiveConnections)
                        .UseDashboardMetric(SqlServerStorage.TotalConnections)
                        .UseDashboardMetric(DashboardMetrics.FailedCount)
                        .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                        .UseDashboardMetric(DashboardMetrics.ScheduledCount)
                        .UseDashboardMetric(DashboardMetrics.EnqueuedCountOrNull);
                }))
                // .AddApplicationInsights()
                .Pipeline(m => m
                    .AddLogging()
                    .AddValidation()
                ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // NOTE: This will only be available locally, use admin website when deployed
            app.UseHangfireDashboard("");
            app.UseHangfireServer();
        }
    }
}
