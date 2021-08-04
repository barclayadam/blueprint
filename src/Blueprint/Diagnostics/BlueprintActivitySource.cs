using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Blueprint.Diagnostics
{
    /// <summary>
    /// Contains an <see cref="ActivitySource" /> for all activity that Blueprint will raise.
    /// </summary>
    public static class BlueprintActivitySource
    {
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
        /// Records an <see cref="Exception" /> against the given <see cref="Activity" />
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

            var activityTagsCollection = new ActivityTagsCollection
            {
                ["exception.type"] = exception.GetType(),
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.ToString(),
            };

            if (escaped == true)
            {
                activityTagsCollection.Add("exception.escaped", "true");
            }

            foreach (var key in exception.Data.Keys.OfType<string>())
            {
                activityTagsCollection.Add(key, exception.Data[key]);
            }

            activity?.AddEvent(new ActivityEvent("exception", tags: activityTagsCollection));
        }
    }
}
