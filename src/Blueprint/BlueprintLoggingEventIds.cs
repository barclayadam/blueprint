using Microsoft.Extensions.Logging;

namespace Blueprint;

/// <summary>
/// Logging event IDs for the Blueprint library.
/// </summary>
internal static class BlueprintLoggingEventIds
{
    /// <summary>
    /// When an API operation is executing.
    /// </summary>
    internal static readonly EventId ApiOperationExecuting = new(1, nameof(ApiOperationExecuting));

    /// <summary>
    /// When an operation has finished executing.
    /// </summary>
    public static readonly EventId ApiOperationFinished = new(2, nameof(ApiOperationFinished));

    /// <summary>
    /// When operation validation fails.
    /// </summary>
    public static readonly EventId ValidationFailed = new(3, nameof(ValidationFailed));

    /// <summary>
    /// When a non-500 exception is caught in a pipeline.
    /// </summary>
    internal static readonly EventId Non500Exception = new(4, nameof(Non500Exception));

    /// <summary>
    /// When an unhandled exception is caught in a pipeline.
    /// </summary>
    internal static readonly EventId UnhandledException = new(5, nameof(UnhandledException));
}
