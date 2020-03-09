using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tests.Tasks.TaskRunner_Middleware
{
    public class InMemoryBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private readonly List<BackgroundTaskEnvelope> enqueued = new List<BackgroundTaskEnvelope>();
        private readonly List<(BackgroundTaskEnvelope, TimeSpan)> scheduled = new List<(BackgroundTaskEnvelope, TimeSpan)>();
        private readonly List<(BackgroundTaskEnvelope, string, JobContinuationOptions)> childEnqueued = new List<(BackgroundTaskEnvelope, string, JobContinuationOptions)>();

        public List<BackgroundTaskEnvelope> Enqueued => enqueued;

        public List<(BackgroundTaskEnvelope, TimeSpan)> Scheduled => scheduled;

        public List<(BackgroundTaskEnvelope, string, JobContinuationOptions)> ChildEnqueued => childEnqueued;

        public Task<string> EnqueueAsync(BackgroundTaskEnvelope task)
        {
            Enqueued.Add(task);

            return Task.FromResult(Enqueued.Count.ToString());
        }

        public Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay)
        {
            Scheduled.Add((task, delay));

            return Task.FromResult(Scheduled.Count.ToString());
        }

        public Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, JobContinuationOptions options)
        {
            ChildEnqueued.Add((task, parentId, options));

            return Task.FromResult(ChildEnqueued.Count.ToString());
        }
    }
}