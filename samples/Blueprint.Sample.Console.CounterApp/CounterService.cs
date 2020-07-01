using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Sample.Console.CounterApp.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint.Sample.Console.CounterApp
{
    public class CounterService : IHostedService, IDisposable
    {
        private readonly IOptionsMonitor<CounterConfiguration> configuration;
        private readonly IApiOperationExecutor apiOperationExecutor;
        private readonly ILogger logger;

        private Timer timer;

        public CounterService(
            IOptionsMonitor<CounterConfiguration> configuration,
            IApiOperationExecutor apiOperationExecutor,
            ILogger<CounterService> logger)
        {
            this.configuration = configuration;
            this.apiOperationExecutor = apiOperationExecutor;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting CounterApp!");

            var max = configuration.CurrentValue.Max;

            timer = new Timer(s => OnTimerElapsed((int)s), max, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        public void OnTimerElapsed(int max)
        {
            apiOperationExecutor.ExecuteWithNewScopeAsync(new IncrementCountCommand
            {
                Max = max
            }).Wait();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogWarning("Stopping CounterApp!");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
