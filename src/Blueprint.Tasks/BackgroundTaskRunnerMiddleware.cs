using Blueprint.Compiler.Frames;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Middleware builder to inject a call to <see cref="IBackgroundTaskScheduler.RunNowAsync" /> for the
    /// current (container-derived) instance of <see cref="IBackgroundTaskScheduler" />.
    /// </summary>
    public class BackgroundTaskRunnerMiddleware : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public bool SupportsNestedExecution => false;

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        /// <param name="operation">The operation to check.</param>
        /// <returns><c>true</c>.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            var runNowCall = MethodCall.For<IBackgroundTaskScheduler>(s => s.RunNowAsync());

            context.AppendFrames(runNowCall);
        }
    }
}
