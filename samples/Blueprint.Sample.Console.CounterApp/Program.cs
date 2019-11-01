using System.IO;
using System.Threading.Tasks;
using Blueprint.Api.Configuration;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blueprint.Sample.Console.CounterApp
{
    public class Program
    {
        public const string AppName = "CounterApp";

        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            await host.RunAsync();
        }

        // EF Core currently needs a method called 'CreateWebHostBuilder' to scaffold migrations
        public static IHostBuilder CreateWebHostBuilder(string[] args) => CreateHostBuilder(args);

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .UseLamar()
                .UseConsoleLifetime()
                .UseDotNetCore2Environment()
                .ConfigureHostConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());

                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    context.HostingEnvironment.ApplicationName = AppName;

                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configure Options
                    services.AddOptions();
                    services.Configure<CounterConfiguration>(context.Configuration);

                    // Configure Blueprint API
                    services.AddBlueprintApi(b => b
                        .SetApplicationName(AppName)
                        .ScanForOperations(typeof(Program).Assembly)
                        .AddBackgroundTasks(a => a.UseHangfire())
                        .Pipeline(m => m
                            .AddLogging()
                            .AddValidation(v => v
                                .UseBlueprintSource()
                                .UseDataAnnotationSource()
                            )
                            .AddAuditing(a => a
                                .StoreInSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=blueprint-examples;Integrated Security=True", "AuditTrail")
                            )
                            .AddTaskRunner()
                        ));

                    // Configure Hosted Services
                    services.AddHostedService<CounterService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                })
                .ConfigureContainer((HostBuilderContext context, ServiceRegistry services) =>
                {
                    // Configure Lamar
                    services.Scan(x =>
                    {
                        x.AssembliesFromApplicationBaseDirectory();
                        x.LookForRegistries();
                    });
                });
    }
}
