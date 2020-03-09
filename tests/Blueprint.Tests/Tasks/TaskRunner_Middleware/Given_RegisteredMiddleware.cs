using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Tasks;
using Blueprint.Testing;
using Blueprint.Tests.Api;
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

            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<TaskOperation>()
                .Configure(c => c.AddTasksClient(p => p.UseInMemory(inMemoryProvider)))
                .Pipeline(p => p.AddLogging()));

            // Act
            var context = executor.ContextFor(new TaskOperation { ToReturn = toReturn });
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(toReturn);
        }

        public class TaskOperation : IApiOperation
        {
            public object ToReturn { get; set; }

            public object Invoke(IBackgroundTaskScheduler taskScheduler)
            {
                taskScheduler.EnqueueAsync(new TestBackgroundTask());

                return ToReturn;
            }
        }

        public class TestBackgroundTask : IBackgroundTask {}
    }
}
