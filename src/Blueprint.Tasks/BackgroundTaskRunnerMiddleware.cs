using Blueprint.Compiler.Frames;
using Blueprint.Tasks;

namespace Blueprint.Api.Middleware
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

        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var runNowCall = MethodCall.For<IBackgroundTaskScheduler>(s => s.RunNowAsync());

            context.AppendFrames(runNowCall);
        }
    }
}
