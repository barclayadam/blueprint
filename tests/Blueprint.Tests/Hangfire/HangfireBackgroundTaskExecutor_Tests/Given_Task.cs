using System;
using Blueprint.Core.Tasks;
using Blueprint.Hangfire;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Blueprint.Tests.Hangfire.HangfireBackgroundTaskExecutor_Tests
{
    using System.Threading.Tasks;

    public class Given_Task
    {
        private const string QueueName = "somequeue";

        class NoPropertiesTask : BackgroundTask { }

        class PropertiesTask : BackgroundTask
        {
            public string Prop1 { get; set; }
        }

        [Queue(QueueName)]
        class QueueAttributeTask : BackgroundTask { }

        [Test]
        public void When_No_Properties_Enqueued_Then_Sets_DisplayName()
        {
            // Act
            var displayName = new NoPropertiesTask().ToString();

            // Assert
            displayName.Should().Be("NoPropertiesTask(\"Metadata\":{})");
        }

        [Test]
        public void When_Properties_Enqueued_Then_Sets_DisplayName()
        {
            // Act
            var displayName = new PropertiesTask
            {
                Prop1 = "A prop"
            }.ToString();

            // Assert
            displayName.Should().Be("PropertiesTask(\"Prop1\":\"A prop\",\"Metadata\":{})");
        }

        [Test]
        public async Task When_NoQueueAttribute_Then_Sets_Queue_To_Default()
        {
            // Arrange
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(new Lazy<IBackgroundJobClient>(() => backgroundJobClient.Object), new NullLogger<HangfireBackgroundTaskScheduleProvider>());

            // Act
            await backgroundTaskExecutor.EnqueueAsync(new NoPropertiesTask());

            // Assert
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == EnqueuedState.DefaultQueue)));
        }

        [Test]
        public async Task When_QueueAttribute_Then_Sets_Queue()
        {
            // Arrange
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var backgroundTaskExecutor = new HangfireBackgroundTaskScheduleProvider(new Lazy<IBackgroundJobClient>(() => backgroundJobClient.Object), new NullLogger<HangfireBackgroundTaskScheduleProvider>());

            // Act
            await backgroundTaskExecutor.EnqueueAsync(new QueueAttributeTask());

            // Assert
            backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.Is<EnqueuedState>(s => s.Queue == QueueName)));
        }
    }
}
