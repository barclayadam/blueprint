using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Api.CodeGen;
using Blueprint.Api.Configuration;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api
{
    public class ApiOperationExecutorBuilder
    {
        private readonly ILogger<ApiOperationExecutorBuilder> logger;

        private readonly HashSet<Assembly> references = new HashSet<Assembly>();
        private readonly List<IMiddlewareBuilder> builders = new List<IMiddlewareBuilder>();

        public ApiOperationExecutorBuilder(ILogger<ApiOperationExecutorBuilder> logger)
        {
            this.logger = logger;

            references.Add(typeof(ApiOperationExecutorBuilder).Assembly);
        }

        /// <summary>
        /// Given the specified <see cref="BlueprintApiOptions" /> will generate and compile an
        /// <see cref="IApiOperationExecutor" /> that can be used to execute any operation that
        /// has been identified by the model of the options passed.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public CodeGennedExecutor Build(BlueprintApiOptions options, IServiceProvider serviceProvider)
        {
            logger.LogInformation("Building CodeGennedExecutor for {0} operations", options.Model.Operations.Count());

            using (var serviceScope = serviceProvider.CreateScope())
            {
                foreach (var middleware in options.Middlewares)
                {
                    Use((IMiddlewareBuilder)Activator.CreateInstance(middleware));
                }

                var model = options.Model;
                var dictionary = new Dictionary<Type, Func<IOperationExecutorPipeline>>();

                // Start the definition for a new generated assembly
                var assembly = new GeneratedAssembly(options.Rules);

                foreach (var operation in model.Operations)
                {
                    references.Add(operation.OperationType.Assembly);
                }

                foreach (var a in references)
                {
                    logger.LogDebug("Referencing assembly {0}", a.FullName);

                    assembly.ReferenceAssembly(a);
                }

                foreach (var operation in model.Operations)
                {
                    logger.LogDebug("Generating executor for {0}", operation.OperationType.FullName);

                    var pipelineExecutorType = assembly.AddType(
                        $"{GetLastNamespaceSegment(operation)}{operation.OperationType.Name}Executor",
                        typeof(IOperationExecutorPipeline));

                    var executeMethod = pipelineExecutorType.MethodFor(nameof(IOperationExecutorPipeline.ExecuteAsync));

                    var operationContextVariable = executeMethod.Arguments[0];

                    var castFrame = new ConcreteOperationCastFrame(operationContextVariable, operation.OperationType);

                    var apiOperationContextSource =
                        new ApiOperationContextVariableSource(operationContextVariable, castFrame.CastOperationVariable);

                    var instanceFrameProvider = serviceProvider.GetService<IInstanceFrameProvider>() ?? DefaultInstanceFrameProvider.Instance;

                    var context = new MiddlewareBuilderContext(
                        executeMethod,
                        apiOperationContextSource,
                        operation,
                        model,
                        serviceScope.ServiceProvider,
                        instanceFrameProvider);

                    context.RegisterUnhandledExceptionHandler(typeof(Exception), e => new[] { new BaseExceptionCatchFrame(context, e) });

                    executeMethod.Sources.Add(apiOperationContextSource);

                    var executorLoggerName = $"{options.ApplicationName}.{GetLastNamespaceSegment(operation)}.{operation.OperationType.Name}Executor";
                    var executorLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(executorLoggerName);
                    pipelineExecutorType.AllInjectedFields.Add(new LoggerVariable(executorLoggerName, executorLogger));

                    executeMethod.Frames.Add(castFrame);
                    executeMethod.Frames.Add(new ErrorHandlerFrame(context));
                    executeMethod.Frames.Add(new BlankLineFrame());

                    foreach (var behaviour in builders)
                    {
                        if (behaviour.Matches(operation))
                        {
                            executeMethod.Frames.Add(new CommentFrame(behaviour.GetType().Name));

                            try
                            {
                                behaviour.Build(context);
                            }
                            catch (Exception ex)
                            {
                                executorLogger.LogCritical(ex, $"An unhandled exception occurred in middleware builder {behaviour.GetType()}");

                                throw new InvalidOperationException(
                                    $"An unhandled exception occurred in middleware builder {behaviour.GetType()}", ex);
                            }

                            executeMethod.Frames.Add(new BlankLineFrame());
                        }
                    }

                    dictionary.Add(operation.OperationType, () => (IOperationExecutorPipeline)ActivatorUtilities.CreateInstance(serviceProvider, pipelineExecutorType.CompiledType));
                }

                try
                {
                    logger.LogInformation("Compiling {0} pipeline executors", dictionary.Count);

                    assembly.CompileAll(serviceProvider.GetRequiredService<AssemblyGenerator>());

                    logger.LogInformation("Done compiling {0} pipeline executors", dictionary.Count);
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Failed to compile pipeline executors");

                    throw;
                }

                return new CodeGennedExecutor(serviceProvider, model, assembly, dictionary.ToDictionary(d => d.Key, d => d.Value()));
            }
        }

        private static string GetLastNamespaceSegment(ApiOperationDescriptor operation)
        {
            return operation.OperationType.Namespace == null ? string.Empty : operation.OperationType.Namespace.Split('.').Last();
        }

        /// <summary>
        /// Uses the given <see cref="IMiddlewareBuilder" />, placing it at the end of the
        /// current list and adding the assembly from where it comes from as a reference.
        /// </summary>
        /// <remarks>
        /// This method is not normally used directly, as the building is directed by the options
        /// present in <see cref="BlueprintApiOptions" />.
        /// </remarks>
        /// <param name="middlewareBuilder">The builder to add, must not be <c>null</c>.</param>
        private void Use(IMiddlewareBuilder middlewareBuilder)
        {
            Guard.NotNull(nameof(middlewareBuilder), middlewareBuilder);

            builders.Add(middlewareBuilder);
            references.Add(middlewareBuilder.GetType().Assembly);
        }

        /// <summary>
        /// A frame that will output the code to create a variable of the "proper" operation type, meaning that
        /// we do not need to constantly perform a cast on <see cref="ApiOperationContext.Operation" /> whenever we need
        /// the concrete type.
        /// </summary>
        private class ConcreteOperationCastFrame : SyncFrame
        {
            private readonly Argument operationContextVariable;
            private readonly Variable operationVariable;

            public ConcreteOperationCastFrame(Argument operationContextVariable, Type operationType)
            {
                this.operationContextVariable = operationContextVariable;

                operationVariable = new Variable(operationType, this);
            }

            public Variable CastOperationVariable => operationVariable;

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"var {operationVariable} = ({operationVariable.VariableType.FullNameInCode()}) {operationContextVariable.GetProperty(nameof(ApiOperationContext.Operation))};");

                Next?.GenerateCode(method, writer);
            }
        }
    }
}
