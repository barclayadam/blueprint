using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Blueprint.Diagnostics
{
    /// <summary>
    /// Contains an <see cref="ActivitySource" /> for all activity that Blueprint will raise.
    /// </summary>
    public static class BlueprintActivitySource
    {
        internal static readonly AssemblyName AssemblyName = typeof(BlueprintActivitySource).Assembly.GetName();
        internal static readonly string ActivitySourceName = AssemblyName.Name;
        internal static readonly Version Version = AssemblyName.Version;

        /// <summary>
        /// The singular <see cref="ActivitySource" /> used throughout the Blueprint library.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new (ActivitySourceName, Version.ToString());

        public static ActivitySource CreateChild(Type assemblyType, string name)
        {
            return new ActivitySource($"{ActivitySourceName}.{name}", assemblyType.Assembly.GetName().Version.ToString());
        }

        /// <summary>
        /// Records an <see cref="Exception" /> against the given <see cref="Activity" />
        /// </summary>
        /// <param name="activity">The <see cref="Activity" /> to record the exception to.</param>
        /// <param name="exception">The exception to record.</param>
        public static void RecordException([CanBeNull] Activity activity, Exception exception)
        {
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                ["exception.type"] = exception.GetType(),
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.ToString(),
            }));
        }
    }
}
