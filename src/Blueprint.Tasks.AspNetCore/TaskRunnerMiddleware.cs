using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Tasks.AspNetCore;

/// <summary>
/// An ASP.NET Core middleware that will call <see cref="IBackgroundTaskScheduler.RunNowAsync" /> for the
/// current instance of <see cref="IBackgroundTaskScheduler" /> after subsequent middleware has been
/// executed.
/// </summary>
public class TaskRunnerMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskRunnerMiddleware" />.
    /// </summary>
    /// <param name="next">The next delegate / middleware to call.</param>
    public TaskRunnerMiddleware(RequestDelegate next)
    {
        Guard.NotNull(nameof(next), next);

        this._next = next;
    }

    /// <summary>
    /// Executes the next middleware, followed by calling <see cref="IBackgroundTaskScheduler.RunNowAsync" />.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="backgroundTaskScheduler">The background task scheduler</param>
    /// <returns>A task representing this invocation.</returns>
    public async Task Invoke(HttpContext context, IBackgroundTaskScheduler backgroundTaskScheduler)
    {
        await this._next(context);
        await backgroundTaskScheduler.RunNowAsync();
    }
}