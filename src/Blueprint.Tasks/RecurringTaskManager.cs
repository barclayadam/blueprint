using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint.Tasks;

/// <summary>
/// The <see cref="RecurringTaskManager" /> is responsible for interacting with an external task manager
/// and the set of registered <see cref="IRecurringTaskScheduler" />s to create a number of scheduled
/// <see cref="IBackgroundTask" />s.
/// </summary>
public class RecurringTaskManager
{
    private const char IdSplitter = ':';

    private readonly IEnumerable<IRecurringTaskScheduler> _taskSchedulers;
    private readonly IRecurringTaskProvider _provider;
    private readonly ILogger<RecurringTaskManager> _logger;
    private readonly IOptions<RecurringTaskManagerOptions> _options;

    /// <summary>
    /// Initialises a new instance of the <see cref="RecurringTaskManager" /> class.
    /// </summary>
    /// <param name="taskSchedulers">The recurring task schedulers that will be used to create the
    /// schedules.</param>
    /// <param name="provider">The provider that implements the actual scheduling.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The task options.</param>
    public RecurringTaskManager(
        IEnumerable<IRecurringTaskScheduler> taskSchedulers,
        IRecurringTaskProvider provider,
        ILogger<RecurringTaskManager> logger,
        IOptions<RecurringTaskManagerOptions> options)
    {
        Guard.NotNull(nameof(taskSchedulers), taskSchedulers);
        Guard.NotNull(nameof(provider), provider);
        Guard.NotNull(nameof(logger), logger);
        Guard.NotNull(nameof(options), options);

        this._taskSchedulers = taskSchedulers;
        this._provider = provider;
        this._logger = logger;
        this._options = options;
    }

    /// <summary>
    /// Reschedules all recurring tasks and ensured the provider has the _current_ set of
    /// <see cref="RecurringTaskSchedule"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RescheduleAllAsync()
    {
        var schedulers = this.GetActiveSchedulers().ToArray();
        var current = new List<RecurringTaskScheduleDto>();

        this._logger.LogInformation("Rescheduling tasks from {SchedulerCount} schedulers", schedulers.Length);

        foreach (var taskScheduler in schedulers)
        {
            await this.RescheduleAsync(taskScheduler, current);
        }

        await this._provider.UpdateAsync(current);
    }

    private static string GetGroupNameFromScheduler(IRecurringTaskScheduler scheduler)
    {
        return scheduler.GetType().Name;
    }

    private async Task<IEnumerable<RecurringTaskSchedule>> GetSchedulesAsync(IRecurringTaskScheduler recurringTaskScheduler, string schedulerName)
    {
        try
        {
            this._logger.LogInformation("Getting scheduled tasks. scheduler={0}", schedulerName);

            return await recurringTaskScheduler.GetTaskSchedulesAsync();
        }
        catch (Exception e)
        {
            this._logger.LogError(
                e,
                "Unhandled exception getting schedules from '{0}'. Will delete all existing jobs.",
                schedulerName);
        }

        return Enumerable.Empty<RecurringTaskSchedule>();
    }

    /// <summary>
    /// Gets all the schedulers that have been registered and are currently active.
    /// </summary>
    /// <remarks>
    /// We ask for new schedulers every time to ensure that dependencies that should be transient are (e.g. a
    /// database context should be new every time a scheduler is called).
    /// </remarks>
    /// <returns>A list of active schedulers.</returns>
    private IEnumerable<IRecurringTaskScheduler> GetActiveSchedulers()
    {
        var loadedOptions = this._options.Value;
        var schedulerEnabled = loadedOptions.SchedulerEnabled;

        if (!schedulerEnabled)
        {
            return Enumerable.Empty<IRecurringTaskScheduler>();
        }

        return this._taskSchedulers.Where(s => !loadedOptions.DisabledSchedulers.Contains(s.GetType().Name));
    }

    private async Task RescheduleAsync(IRecurringTaskScheduler recurringTaskScheduler, List<RecurringTaskScheduleDto> current)
    {
        var schedulerName = recurringTaskScheduler.GetType().Name;

        this._logger.LogInformation("Getting schedules for scheduler {SchedulerName}", schedulerName);

        var taskSchedules = (await this.GetSchedulesAsync(recurringTaskScheduler, schedulerName)).ToList();

        var group = GetGroupNameFromScheduler(recurringTaskScheduler);

        // Create and update jobs
        foreach (var taskSchedule in taskSchedules)
        {
            var id = group + IdSplitter + taskSchedule.Name;

            current.Add(new RecurringTaskScheduleDto(id, taskSchedule));
        }
    }
}