using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks;

/// <summary>
/// The default implementation of <see cref="IBackgroundTaskScheduler" /> that uses the
/// registered <see cref="IBackgroundTaskScheduleProvider" /> to perform the enqueuing and
/// scheduling of <see cref="IBackgroundTask" />s, implementing a unit of work pattern
/// in which tasks are only "released" once <see cref="IBackgroundTaskScheduler.RunNowAsync" />
/// is executed.
/// </summary>
public class BackgroundTaskScheduler : IBackgroundTaskScheduler
{
    private readonly IBackgroundTaskScheduleProvider _backgroundTaskScheduleProvider;
    private readonly IEnumerable<IBackgroundTaskPreprocessor> _backgroundTaskPreprocessors;
    private readonly ILogger<BackgroundTaskScheduler> _logger;

    // Note that we do not initialise this up front, but instead JIT. This helps reduce the number of allocations
    // in the case of no tasks actually being scheduled (which is likely the majority of calls)
    private List<ScheduledBackgroundTask> _tasks;

    /// <summary>
    /// Initialises a new instance of the <see cref="BackgroundTaskScheduler" /> class.
    /// </summary>
    /// <param name="backgroundTaskPreprocessors">The registered background task preprocessors.</param>
    /// <param name="backgroundTaskScheduleProvider">The provider-specific implementation to delegate to.</param>
    /// <param name="logger">The logger for this class.</param>
    public BackgroundTaskScheduler(
        IEnumerable<IBackgroundTaskPreprocessor> backgroundTaskPreprocessors,
        IBackgroundTaskScheduleProvider backgroundTaskScheduleProvider,
        ILogger<BackgroundTaskScheduler> logger)
    {
        Guard.NotNull(nameof(backgroundTaskScheduleProvider), backgroundTaskScheduleProvider);
        Guard.NotNull(nameof(logger), logger);

        this._backgroundTaskScheduleProvider = backgroundTaskScheduleProvider;
        this._backgroundTaskPreprocessors = backgroundTaskPreprocessors;
        this._logger = logger;
    }

    /// <inheritdoc />
    public IScheduledBackgroundTask Enqueue(IBackgroundTask task)
    {
        this._tasks ??= new List<ScheduledBackgroundTask>();

        var envelope = this.CreateTaskEnvelope(task);
        var scheduledTask = new ScheduledBackgroundTask(envelope, null, this);

        this._tasks.Add(scheduledTask);

        return scheduledTask;
    }

    /// <inheritdoc />
    public IScheduledBackgroundTask Schedule(IBackgroundTask task, TimeSpan delay)
    {
        this._tasks ??= new List<ScheduledBackgroundTask>();

        var envelope = this.CreateTaskEnvelope(task);
        var scheduledTask = new ScheduledBackgroundTask(envelope, delay, this);

        this._tasks.Add(scheduledTask);

        return scheduledTask;
    }

    /// <inheritdoc />
    public ValueTask RunNowAsync()
    {
        if (this._tasks == null || this._tasks.Count == 0)
        {
            return default;
        }

        // Clearing tasks before executing so any more calls to execute tasks doesn't re-execute same tasks
        var currentTasks = new List<ScheduledBackgroundTask>(this._tasks);

        this._tasks.Clear();

        if (currentTasks.Count > 5)
        {
            this._logger.LogWarning(
                "Queuing a large number of tasks, considering reducing this number through consolidating to avoid potentially slow scheduling. Queuing {0} tasks",
                currentTasks.Count);
        }

        return new ValueTask(this.PushAllAsync(currentTasks));
    }

    private async Task PushAllAsync(List<ScheduledBackgroundTask> tasks)
    {
        foreach (var task in tasks)
        {
            await task.PushToProviderAsync(this._backgroundTaskScheduleProvider);
        }
    }

    private BackgroundTaskEnvelope CreateTaskEnvelope<T>(T task) where T : IBackgroundTask
    {
        var envelope = new BackgroundTaskEnvelope(task);

        foreach (var p in this._backgroundTaskPreprocessors)
        {
            p.Preprocess(envelope);
        }

        return envelope;
    }
}