using System;
using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Blueprint.Tasks.Hangfire;

/// <summary>
/// A JobFilter that will apply any AutomatedRetry job filter attribute for a task (applied to
/// the class itself) to the execution of that task.
/// </summary>
/// <remarks>
/// This delegation attribute is required because there is no way to markup in a dynamic way
/// for tasks as they all execute through a single method.
/// </remarks>
public class TaskAutomaticRetryJobFilter : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    private readonly AutomaticRetryAttribute _defaultAutomaticRetryAttribute;

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskAutomaticRetryJobFilter" /> class.
    /// </summary>
    /// <param name="defaultAutomaticRetryAttribute">The default registered <see cref="AutomaticRetryAttribute" /> that
    /// will be applied.</param>
    public TaskAutomaticRetryJobFilter(AutomaticRetryAttribute defaultAutomaticRetryAttribute)
    {
        this._defaultAutomaticRetryAttribute = defaultAutomaticRetryAttribute;
    }

    /// <inheritdoc />
    public void OnStateElection(ElectStateContext context)
    {
        this.WithRetryAttribute(context.BackgroundJob, a => a.OnStateElection(context));
    }

    /// <inheritdoc />
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        this.WithRetryAttribute(context.BackgroundJob, a => a.OnStateApplied(context, transaction));
    }

    /// <inheritdoc />
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        this.WithRetryAttribute(context.BackgroundJob, a => a.OnStateUnapplied(context, transaction));
    }

    private static Type GetTaskType(BackgroundJob backgroundJob)
    {
        // We have an unknown type of Jb (i.e. after name refactor, removal) so this could
        // not be processed, treat as a non-task job.
        if (backgroundJob.Job == null)
        {
            return null;
        }

        if (backgroundJob.Job.Type == typeof(TaskExecutor) && backgroundJob.Job.Args[0] != null)
        {
            // We rely on the fact that the TaskExecutor takes a single argument that is the
            // actual task, and that this task has been serialised to JSON with full type
            // information.
            return backgroundJob.Job.Args[0].GetType();
        }

        return null;
    }

    private void WithRetryAttribute(BackgroundJob backgroundJob, Action<AutomaticRetryAttribute> onAttribute)
    {
        var taskType = GetTaskType(backgroundJob);

        var retryAttribute = taskType?.GetCustomAttribute<AutomaticRetryAttribute>();

        if (retryAttribute != null)
        {
            onAttribute(retryAttribute);
            return;
        }

        // Either this is not a task being executed, or the task itself has not had an
        // attribute applied. In this case let's apply the global default.
        if (this._defaultAutomaticRetryAttribute != null)
        {
            onAttribute(this._defaultAutomaticRetryAttribute);
        }
    }
}