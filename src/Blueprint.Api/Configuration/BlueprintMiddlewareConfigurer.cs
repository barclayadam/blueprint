using System.Collections.Generic;
using Blueprint.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintMiddlewareConfigurer
    {
        private readonly List<MiddlewareRegistration> middlewareStages = new List<MiddlewareRegistration>();

        private readonly BlueprintApiConfigurer blueprintApiConfigurer;

        internal BlueprintMiddlewareConfigurer(BlueprintApiConfigurer blueprintApiConfigurer)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;

            Add(new OperationExecutorMiddlewareBuilder(), MiddlewareStage.Execution, 0);

            // Special-case this last middleware to have high priority as we MUST have this as the very last middleware, regardless of what else will
            // be registered
            Add(new FormatterMiddlewareBuilder(), MiddlewareStage.PostExecution, int.MaxValue);
        }

        public IServiceCollection Services => blueprintApiConfigurer.Services;

        public BlueprintMiddlewareConfigurer AddMiddlewareBefore<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
        {
            Add(new T(), middlewareStage, -1);

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddlewareBefore(IMiddlewareBuilder builder, MiddlewareStage middlewareStage)
        {
            Add(builder, middlewareStage, -1);

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddleware<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
        {
            Add(new T(), middlewareStage, 0);

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddleware(IMiddlewareBuilder builder, MiddlewareStage middlewareStage)
        {
            Add(builder, middlewareStage, 0);

            return this;
        }

        internal void Register()
        {
            middlewareStages.Sort((x, y) =>
            {
                if (x.Stage < y.Stage)
                {
                    return -1;
                }

                if (x.Stage == y.Stage)
                {
                    if (x.Priority == y.Priority)
                    {
                        // We have same stage and priority so use the index added to maintain order as List<T>.Sort is NOT
                        // a stable sort
                        return x.Index.CompareTo(y.Index);
                    }

                    return x.Priority.CompareTo(y.Priority);
                }

                return 1;
            });

            foreach (var registration in middlewareStages)
            {
                blueprintApiConfigurer.Options.UseMiddlewareBuilder(registration.MiddlewareBuilder);
            }
        }

        private void Add(IMiddlewareBuilder middleware, MiddlewareStage middlewareStage, int priority)
        {
            middlewareStages.Add(new MiddlewareRegistration(middleware, middlewareStage, priority, middlewareStages.Count));
        }

        private class MiddlewareRegistration
        {
            public MiddlewareRegistration(IMiddlewareBuilder middlewareBuilder, MiddlewareStage stage, int priority, int index)
            {
                MiddlewareBuilder = middlewareBuilder;
                Stage = stage;
                Priority = priority;
                Index = index;
            }

            internal IMiddlewareBuilder MiddlewareBuilder { get; }

            internal MiddlewareStage Stage { get; }

            internal int Priority { get; }

            internal int Index { get; }

            public override string ToString()
            {
                return $"{MiddlewareBuilder.GetType().Name} in {Stage} at priority {Priority}/{Index}";
            }
        }
    }
}
