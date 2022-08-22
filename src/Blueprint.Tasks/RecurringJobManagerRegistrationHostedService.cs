using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Tasks;

/// <summary>
/// An <see cref="IHostedService" /> that is responsible, on startup of the application, to install
/// the recurring task manager service using <see cref="IRecurringTaskProvider.SetupRecurringManagerAsync" />.
/// </summary>
internal class RecurringJobManagerRegistrationHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;

    public RecurringJobManagerRegistrationHostedService(IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider)
    {
        this._appLifetime = appLifetime;
        this._serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._appLifetime.ApplicationStarted.Register(this.OnStarted);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async void OnStarted()
    {
        using var scope = this._serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IRecurringTaskProvider>();

        await provider.SetupRecurringManagerAsync();
    }
}