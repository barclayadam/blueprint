﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Blueprint.Api
{
    public class ApiOperationExecutorBuilder
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly HashSet<Assembly> references = new HashSet<Assembly>();
        private readonly List<IMiddlewareBuilder> behaviours = new List<IMiddlewareBuilder>();

        public ApiOperationExecutorBuilder()
        {
            references.Add(typeof(ApiOperationExecutorBuilder).Assembly);
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
        public void Use(IMiddlewareBuilder middlewareBuilder)
        {
            Guard.NotNull(nameof(middlewareBuilder), middlewareBuilder);

            behaviours.Add(middlewareBuilder);
            references.Add(middlewareBuilder.GetType().Assembly);
        }

        /// <summary>
        /// References the given <see cref="Assembly" /> when compiling the pipelines in <see cref="Build"/>.
        /// </summary>
        /// <param name="assembly">The assembly to reference, must not be <c>null</c>.</param>
        public void Reference(Assembly assembly)
        {
            Guard.NotNull(nameof(assembly), assembly);

            references.Add(assembly);
        }

        /// <summary>
        /// Given the specified <see cref="BlueprintApiOptions" /> will generate and compiler an
        /// <see cref="IApiOperationExecutor" /> that can be used to execute any operation that
        /// has been identified by the model of the options passed.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public CodeGennedExecutor Build(BlueprintApiOptions options, IServiceProvider serviceProvider)
        {
            Log.Info("Building CodeGennedExecutor for {0} operations", options.Model.Operations.Count());

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
                    Log.Debug("Referencing assembly {0}", a.FullName);

                    assembly.ReferenceAssembly(a);
                }

                foreach (var operation in model.Operations)
                {
                    Log.Debug("Generating executor for {0}", operation.OperationType.FullName);

                    var executor = assembly.AddType(
                        $"{GetLastNamespaceSegment(operation)}{operation.OperationType.Name}Executor",
                        typeof(IOperationExecutorPipeline));

                    var executeMethod = executor.MethodFor(nameof(IOperationExecutorPipeline.ExecuteAsync));

                    var operationContextVariable = executeMethod.Arguments[0];

                    var castFrame = new ConcreteOperationCastFrame(operationContextVariable, operation.OperationType);

                    var apiOperationContextSource =
                        new ApiOperationContextVariableSource(operationContextVariable, castFrame.CastOperationVariable);

                    var instanceFrameProvider = serviceProvider.GetService<IInstanceFrameProvider>() ?? DefaultInstanceFrameProvider.Instance;

                    var context = new MiddlewareBuilderContext(
                        assembly,
                        apiOperationContextSource,
                        executor,
                        executeMethod,
                        operation,
                        model,
                        serviceScope.ServiceProvider,
                        instanceFrameProvider);

                    context.RegisterUnhandledExceptionHandler(typeof(Exception), e => new[] { new BaseExceptionCatchFrame(context, e) });

                    executeMethod.Sources.Add(apiOperationContextSource);

                    executor.AllStaticFields.Add(new LoggerVariable(CreateLoggerForExecutor(options, operation)));

                    executeMethod.Frames.Add(castFrame);
                    executeMethod.Frames.Add(new ErrorHandlerFrame(context));
                    executeMethod.Frames.Add(new BlankLineFrame());

                    foreach (var behaviour in behaviours)
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
                                Log.Fatal(ex, $"An unhandled exception occurred in middleware builder {behaviour.GetType()}");

                                throw new InvalidOperationException(
                                    $"An unhandled exception occurred in middleware builder {behaviour.GetType()}", ex);
                            }

                            executeMethod.Frames.Add(new BlankLineFrame());
                        }
                    }

                    dictionary.Add(operation.OperationType, () => (IOperationExecutorPipeline)ActivatorUtilities.CreateInstance(serviceProvider, executor.CompiledType));
                }

                try
                {
                    Log.Info("Compiling {0} pipeline executors", dictionary.Count);

                    assembly.CompileAll();

                    Log.Info("Done compiling {0} pipeline executors", dictionary.Count);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);

                    throw;
                }

                return new CodeGennedExecutor(serviceProvider, model, assembly, dictionary.ToDictionary(d => d.Key, d => d.Value()));
            }
        }

        private static Logger CreateLoggerForExecutor(BlueprintApiOptions options, ApiOperationDescriptor operation)
        {
            return LogManager.GetLogger($"{options.ApplicationName}.{GetLastNamespaceSegment(operation)}.{operation.OperationType.Name}Executor");
        }

        private static string GetLastNamespaceSegment(ApiOperationDescriptor operation)
        {
            return operation.OperationType.Namespace == null ? string.Empty : operation.OperationType.Namespace.Split('.').Last();
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