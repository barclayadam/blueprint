using System.Threading.Tasks;
using Blueprint.Api.Configuration;
using Blueprint.Tasks;
using Blueprint.Testing;
using Blueprint.Tests.Tasks.TaskRunner_Middleware;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Tasks
{
    public class Given_Task_Server
    {
        [Test]
        public async Task When_Executing_BackgroundTask_Can_Omit_Return()
        {
            // Arrange
            var inMemoryProvider = new InMemoryBackgroundTaskScheduleProvider();
            var testBackgroundTask = new TestBackgroundTask();

            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<TestBackgroundTask>()
                .Configure(c => c.AddTasksServer(p => p.UseInMemory(inMemoryProvider))));

            // Act
            var context = executor.ContextFor(testBackgroundTask);
            await executor.ExecuteAsync(context);

            // Assert
            testBackgroundTask.HasExecuted.Should().BeTrue();
        }

        public class TestBackgroundTask : IBackgroundTask
        {
            public bool HasExecuted { get; private set; } = false;

            public void Invoke()
            {
                HasExecuted = true;
            }
        }
    }
}
