using System;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Tasks;
using Blueprint.Tasks.Hangfire;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using JobContinuationOptions = Hangfire.JobContinuationOptions;

namespace Blueprint.Tests.Tasks.Hangfire.HangfireBackgroundTaskExecutor_Tests;

public class Given_Child_Tasks
{
    class ChildTask : IBackgroundTask { }

    class ParentTask : IBackgroundTask { }

    [Test]
    [TestCase(JobContinuationOptions.OnlyOnSucceededState)]
    [TestCase(JobContinuationOptions.OnAnyFinishedState)]
    public async Task When_Child_Task_Is_Queued(JobContinuationOptions hangfireOptions)
    {
        // Arrange
        var parentId = Guid.NewGuid().ToString();
        var backgroundJobClient = new Mock<IBackgroundJobClient>();
        var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(backgroundJobClient.Object, new NullLogger<HangfireBackgroundTaskScheduleProvider>());
        var backgroundTaskScheduler = new BackgroundTaskScheduler(Enumerable.Empty<IBackgroundTaskPreprocessor>(), backgroundTaskExecutor, new NullLogger<BackgroundTaskScheduler>());
        backgroundJobClient.Setup(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)))
            .Returns(parentId).Verifiable();
        backgroundJobClient.Setup(c => c.Create(It.IsAny<Job>(), It.Is<AwaitingState>(s => s.ParentId == parentId && s.Options == hangfireOptions)))
            .Verifiable();

        // Act
        var parentTask = backgroundTaskScheduler.Enqueue(new ParentTask());
        var childTask = parentTask.ContinueWith(new ChildTask(),
            hangfireOptions == JobContinuationOptions.OnlyOnSucceededState
                ? BackgroundTaskContinuationOptions.OnlyOnSucceededState
                : BackgroundTaskContinuationOptions.OnAnyFinishedState);
        await backgroundTaskScheduler.RunNowAsync();

        // Assert
        parentTask.Should().NotBeNull();
        childTask.Should().NotBeNull();
        backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)));
        backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<AwaitingState>(s => s.ParentId == parentId && s.Options == hangfireOptions)));
    }
}