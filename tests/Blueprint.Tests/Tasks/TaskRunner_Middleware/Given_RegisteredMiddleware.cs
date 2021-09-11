using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Tasks.TaskRunner_Middleware
{
    public class Given_RegisteredMiddleware
    {
        [Test]
        public async Task When_Inline_Handler_Request_IBackgroundTaskScheduler_Then_Can_Compile()
        {
            // Arrange
            var inMemoryProvider = new InMemoryBackgroundTaskScheduleProvider();
            var toReturn = 12345;

            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithOperation<TaskOperation>()
                .AddTasksClient(p => p.UseInMemory(inMemoryProvider))
                .AddLogging());

            // Act
            var result = await executor.ExecuteAsync(new TaskOperation { ToReturn = toReturn });

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
        }

        public class TaskOperation
        {
            public object ToReturn { get; set; }

            public object Invoke(IBackgroundTaskScheduler taskScheduler)
            {
                taskScheduler.Enqueue(new TestBackgroundTask());

                return ToReturn;
            }
        }

        public class TestBackgroundTask : IBackgroundTask {}
    }
}
