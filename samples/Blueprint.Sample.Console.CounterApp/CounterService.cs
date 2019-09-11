using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Sample.Console.CounterApp.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;

namespace Blueprint.Sample.Console.CounterApp
{
    public class CounterService : IHostedService, IDisposable
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IOptionsMonitor<CounterConfiguration> configuration;
        private readonly IApiOperationExecutor apiOperationExecutor;

        private Timer timer;

        public CounterService(IOptionsMonitor<CounterConfiguration> configuration, IApiOperationExecutor apiOperationExecutor)
        {
            this.configuration = configuration;
            this.apiOperationExecutor = apiOperationExecutor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Info("Starting CounterApp!");

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
            Log.Warn("Stopping CounterApp!");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
