using System.Collections.Generic;
using System.Linq;
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

            AddMiddleware<OperationExecutorMiddlewareBuilder>(MiddlewareStage.Execution);
            AddMiddleware<FormatterMiddlewareBuilder>(MiddlewareStage.Execution);
        }

        public IServiceCollection Services => blueprintApiConfigurer.Services;

        public BlueprintMiddlewareConfigurer AddMiddlewareBefore<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
        {
            middlewareStages.Add(new MiddlewareRegistration(new T(), middlewareStage, -1, middlewareStages.Count));

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddleware<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
        {
            middlewareStages.Add(new MiddlewareRegistration(new T(), middlewareStage, 0, middlewareStages.Count));

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddleware(IMiddlewareBuilder builder, MiddlewareStage middlewareStage)
        {
            middlewareStages.Add(new MiddlewareRegistration(builder, middlewareStage, 0, middlewareStages.Count));

            return this;
        }

        public BlueprintMiddlewareConfigurer AddMiddlewareAfter<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
        {
            middlewareStages.Add(new MiddlewareRegistration(new T(), middlewareStage, 1, middlewareStages.Count));

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
                return $"{MiddlewareBuilder.GetType().Name} in {Stage} at priority {Index}/{Priority}";
            }
        }
    }
}
