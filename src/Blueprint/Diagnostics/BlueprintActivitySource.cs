using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace Blueprint.Diagnostics;

/// <summary>
/// Contains an <see cref="ActivitySource" /> for all activity that Blueprint will raise.
/// </summary>
public static class BlueprintActivitySource
{
    private const string TagExceptionEventName = "exception";
    private const string TagExceptionType = "exception.type";
    private const string TagExceptionMessage = "exception.message";
    private const string TagExceptionStacktrace = "exception.stacktrace";
    private const string TagExceptionEscaped = "exception.escaped";

    private static readonly AssemblyName _assemblyName = typeof(BlueprintActivitySource).Assembly.GetName();
    private static readonly Version _version = _assemblyName.Version;

    internal static readonly string ActivitySourceName = _assemblyName.Name;

    /// <summary>
    /// The singular <see cref="ActivitySource" /> used throughout the Blueprint library.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new (ActivitySourceName, _version.ToString());

    public static ActivitySource CreateChild(Type assemblyType, string name)
    {
        return new ActivitySource($"{ActivitySourceName}.{name}", assemblyType.Assembly.GetName().Version.ToString());
    }

    /// <summary>
    /// Records an <see cref="Exception" /> against the given <see cref="Activity" /> as an event.
    /// </summary>
    /// <param name="activity">The <see cref="Activity" /> to record the exception to.</param>
    /// <param name="exception">The exception to record.</param>
    /// <param name="escaped">Whether the exception event is recorded at a point where it is known that the exception is escaping the scope of the span. </param>
    public static void RecordException([CanBeNull] Activity activity, Exception exception, bool? escaped = null)
    {
        if (activity == null)
        {
            return;
        }

        var tags = new ActivityTagsCollection
        {
            [TagExceptionType] = exception.GetType().FullName,
            [TagExceptionStacktrace] = ToInvariantString(exception),
        };

        if (!string.IsNullOrWhiteSpace(exception.Message))
        {
            tags.Add(TagExceptionMessage, exception.Message);
        }

        if (escaped == true)
        {
            tags.Add(TagExceptionEscaped, "true");
        }

        foreach (var key in exception.Data.Keys.OfType<string>())
        {
            tags.Add(key, exception.Data[key]);
        }

        activity?.AddEvent(new ActivityEvent(TagExceptionEventName, tags: tags));
    }

    /// <summary>
    /// Returns a culture-independent string representation of the given <paramref name="exception"/> object,
    /// appropriate for diagnostics tracing.
    /// </summary>
    /// <param name="exception">Exception to convert to string.</param>
    /// <returns>Exception as string with no culture.</returns>
    private static string ToInvariantString(Exception exception)
    {
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;

        try
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            return exception.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }
    }
}