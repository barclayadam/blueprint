using System.Collections.Generic;
using Blueprint.Middleware;

namespace Blueprint.Configuration;

public class PipelineBuilder
{
    private readonly List<MiddlewareRegistration> _middlewareStages = new List<MiddlewareRegistration>();

    private readonly BlueprintApiBuilder _blueprintApiBuilder;

    internal PipelineBuilder(BlueprintApiBuilder blueprintApiBuilder)
    {
        this._blueprintApiBuilder = blueprintApiBuilder;

        this.AddMiddleware<MessagePopulationMiddlewareBuilder>(MiddlewareStage.Population);

        this.Add(new OperationExecutorMiddlewareBuilder(), MiddlewareStage.Execution, 0);

        // Special-case this last middleware to have high priority as we MUST have this as the very last middleware, regardless of what else will
        // be registered
        this.Add(new ReturnFrameMiddlewareBuilder(), MiddlewareStage.PostExecution, int.MaxValue);
    }

    public PipelineBuilder AddMiddlewareBefore<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
    {
        this.Add(new T(), middlewareStage, -1);

        return this;
    }

    public PipelineBuilder AddMiddlewareBefore(IMiddlewareBuilder builder, MiddlewareStage middlewareStage)
    {
        this.Add(builder, middlewareStage, -1);

        return this;
    }

    public PipelineBuilder AddMiddleware<T>(MiddlewareStage middlewareStage) where T : IMiddlewareBuilder, new()
    {
        this.Add(new T(), middlewareStage, 0);

        return this;
    }

    public PipelineBuilder AddMiddleware(IMiddlewareBuilder builder, MiddlewareStage middlewareStage)
    {
        this.Add(builder, middlewareStage, 0);

        return this;
    }

    internal void Register()
    {
        this._middlewareStages.Sort((x, y) =>
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

        foreach (var registration in this._middlewareStages)
        {
            this._blueprintApiBuilder.Options.MiddlewareBuilders.Add(registration.MiddlewareBuilder);
        }
    }

    private void Add(IMiddlewareBuilder middleware, MiddlewareStage middlewareStage, int priority)
    {
        this._middlewareStages.Add(new MiddlewareRegistration(middleware, middlewareStage, priority, this._middlewareStages.Count));
    }

    private class MiddlewareRegistration
    {
        public MiddlewareRegistration(IMiddlewareBuilder middlewareBuilder, MiddlewareStage stage, int priority, int index)
        {
            this.MiddlewareBuilder = middlewareBuilder;
            this.Stage = stage;
            this.Priority = priority;
            this.Index = index;
        }

        internal IMiddlewareBuilder MiddlewareBuilder { get; }

        internal MiddlewareStage Stage { get; }

        internal int Priority { get; }

        internal int Index { get; }

        public override string ToString()
        {
            return $"{this.MiddlewareBuilder.GetType().Name} in {this.Stage} at priority {this.Priority}/{this.Index}";
        }
    }
}