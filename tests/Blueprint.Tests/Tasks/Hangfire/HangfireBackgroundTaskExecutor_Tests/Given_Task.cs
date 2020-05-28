using System.Threading.Tasks;
using Blueprint.Tasks.Hangfire;
using Blueprint.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Blueprint.Tests.Tasks.Hangfire.HangfireBackgroundTaskExecutor_Tests
{
    public class Given_Task
    {
        private const string QueueName = "somequeue";

        class NoPropertiesTask : IBackgroundTask { }

        class PropertiesTask : IBackgroundTask
        {
            public string Prop1 { get; set; }
        }

        [Queue(QueueName)]
        class QueueAttributeTask : IBackgroundTask { }

        [Test]
        public void When_No_Properties_Enqueued_Then_Sets_DisplayName()
        {
            // Act
            var displayName = new HangfireBackgroundTaskWrapper(new BackgroundTaskEnvelope(new NoPropertiesTask())).ToString();

            // Assert
            displayName.Should().Be("NoPropertiesTask()");
        }

        [Test]
        public void When_Properties_Enqueued_Then_Sets_DisplayName()
        {
            // Act
            var displayName = new HangfireBackgroundTaskWrapper(new BackgroundTaskEnvelope(new PropertiesTask
            {
                Prop1 = "A prop"
            })).ToString();

            // Assert
            displayName.Should().Be("PropertiesTask(\"Prop1\":\"A prop\")");
        }

        [Test]
        public async Task When_NoQueueAttribute_Then_Sets_Queue_To_Default()
        {
            // Arrange
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(backgroundJobClient.Object, new NullLogger<HangfireBackgroundTaskScheduleProvider>());

            // Act
            await backgroundTaskExecutor.EnqueueAsync(new BackgroundTaskEnvelope(new NoPropertiesTask()));

            // Assert
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)));
        }

        [Test]
        public async Task When_QueueAttribute_Then_Sets_Queue()
        {
            // Arrange
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(backgroundJobClient.Object, new NullLogger<HangfireBackgroundTaskScheduleProvider>());

            // Act
            await backgroundTaskExecutor.EnqueueAsync(new BackgroundTaskEnvelope(new QueueAttributeTask()));

            // Assert
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == QueueName)));
        }
    }
}
