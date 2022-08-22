using Blueprint.Configuration;
using Blueprint.Sample.WebApi.Data;
using Blueprint.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Blueprint.Samples.TaskProcessor;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWeatherDataSource, WeatherDataSource>();

        services.AddOpenTelemetryTracing(
            builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateEmpty().AddService("task-processor"))
                .AddBlueprintInstrumentation()
                .AddConsoleExporter()
                .AddJaegerExporter()
        );

        services.AddHangfire(h =>
        {
            h
                .UseStorage(new SqlServerStorage("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog=blueprint-examples"))
                .UseRecommendedSerializerSettings()
                .UseDashboardMetric(SqlServerStorage.ActiveConnections)
                .UseDashboardMetric(SqlServerStorage.TotalConnections)
                .UseDashboardMetric(DashboardMetrics.FailedCount)
                .UseDashboardMetric(DashboardMetrics.ProcessingCount)
                .UseDashboardMetric(DashboardMetrics.ScheduledCount)
                .UseDashboardMetric(DashboardMetrics.EnqueuedCountOrNull);
        });

        services.AddHangfireServer();

        services.AddBlueprintApi(a => a
            .AddBackgroundTasks(b => b.UseHangfire())
            .Operations(o => o
                .Scan(typeof(Blueprint.Sample.WebApi.Startup).Assembly))
            .AddLogging());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        app.UseHangfireDashboard("");
        app.UseHangfireServer();
    }
}