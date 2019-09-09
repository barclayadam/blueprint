using System;
using System.Threading.Tasks;

using Blueprint.Core.Tasks;
using Blueprint.Core.Tracing;
using Blueprint.Hangfire;

using global::Hangfire;
using global::Hangfire.Common;
using global::Hangfire.States;

using Moq;

using NUnit.Framework;

using Shouldly;

using StructureMap;

using JobContinuationOptions = global::Hangfire.JobContinuationOptions;

namespace Blueprint.Tests.Hangfire.HangfireBackgroundTaskExecutor_Tests
{
    using Blueprint.Core.Apm;

    public class Given_Child_Tasks
    {
        class ChildTask : BackgroundTask { }

        class ParentTask : BackgroundTask { }

        [Test]
        [TestCase(JobContinuationOptions.OnlyOnSucceededState)]
        [TestCase(JobContinuationOptions.OnAnyFinishedState)]
        public async Task When_Child_Task_Is_Queued(JobContinuationOptions hangfireOptions)
        {
            // Arrange
            var parentId = Guid.NewGuid().ToString();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(new Lazy<IBackgroundJobClient>(() => backgroundJobClient.Object));
            var backgroundTaskScheduler = new BackgroundTaskScheduler(new Container(), backgroundTaskExecutor, new NulloVersionInfoProvider(), new NullApmTool());
            backgroundJobClient.Setup(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)))
                .Returns(parentId).Verifiable();
            backgroundJobClient.Setup(c => c.Create(It.IsAny<Job>(), It.Is<AwaitingState>(s => s.ParentId == parentId && s.Options == hangfireOptions)))
                .Verifiable();
            
            // Act
            var parentTask = await backgroundTaskScheduler.EnqueueAsync(new ParentTask());
            var childTask = parentTask.ContinueWith(new ChildTask(),
                hangfireOptions == JobContinuationOptions.OnlyOnSucceededState
                    ? global::Blueprint.Core.Tasks.JobContinuationOptions.OnlyOnSucceededState
                    : global::Blueprint.Core.Tasks.JobContinuationOptions.OnAnyFinishedState);
            await backgroundTaskScheduler.RunNowAsync();
            
            // Assert
            parentTask.ShouldNotBeNull();
            childTask.ShouldNotBeNull();
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)));
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<AwaitingState>(s => s.ParentId == parentId && s.Options == hangfireOptions)));
        }
    }
}