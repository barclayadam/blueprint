using System;
using System.Diagnostics;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Configuration;
using Blueprint.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint
{
    internal class ApiOperationExecutorBuilderHelper
    {
        /// <summary>
        /// Starts a new <see cref="GeneratedAssembly" /> for the given configured options.
        /// </summary>
        /// <param name="options">The configured options.</param>
        /// <returns>A new <see cref="GeneratedAssembly" /> that should be used to add new pipeline handlers to.</returns>
        public static GeneratedAssembly StartAssembly(BlueprintApiOptions options)
        {
            var assembly = new GeneratedAssembly(options.GenerationRules);

            // Always ensure we have base Blueprint assembly
            assembly.ReferenceAssembly(typeof(ApiOperationExecutorBuilderHelper).Assembly);

            // All middleware builders can automatically reference their own types as we add their containing assembly
            // as a reference
            foreach (var middleware in options.MiddlewareBuilders)
            {
                assembly.ReferenceAssembly(middleware.GetType().Assembly);
            }

            return assembly;
        }

        /// <summary>
        /// Builds the pipeline for a single operation.
        /// </summary>
        /// <param name="model">The complete data model.</param>
        /// <param name="options">The configured options.</param>
        /// <param name="operation">The operation to build for.</param>
        /// <param name="assembly">The type to populate with necessary methods.</param>
        /// <param name="services">A service provider.</param>
        public static GeneratedType BuildPipeline(ApiDataModel model, BlueprintApiOptions options, ApiOperationDescriptor operation, GeneratedAssembly assembly, IServiceProvider services)
        {
            var pipelineExecutorType = assembly.AddType(
                operation.PipelineNamespace,
                operation.PipelineClassName,
                typeof(IOperationExecutorPipeline));

            assembly.ReferenceAssembly(operation.OperationType.Assembly);

            // We need to set up a LoggerVariable once, to be shared between methods
            pipelineExecutorType.AllInjectedFields.Add(new LoggerVariable(pipelineExecutorType.TypeName));

            var executeMethod = pipelineExecutorType.MethodFor(nameof(IOperationExecutorPipeline.ExecuteAsync));
            var executeNestedMethod =
                pipelineExecutorType.MethodFor(nameof(IOperationExecutorPipeline.ExecuteNestedAsync));

            Generate(options, executeMethod, operation, model, services, false);
            Generate(options, executeNestedMethod, operation, model, services, true);

            return pipelineExecutorType;
        }

        private static void Generate(
            BlueprintApiOptions options,
            GeneratedMethod executeMethod,
            ApiOperationDescriptor operation,
            ApiDataModel model,
            IServiceProvider services,
            bool isNested)
        {
            var operationContextVariable = executeMethod.Arguments[0];

            var instanceFrameProvider = services.GetRequiredService<InstanceFrameProvider>();
            var dependencyInjectionVariableSource =
                new DependencyInjectionVariableSource(executeMethod, instanceFrameProvider);

            var castFrame = new ConcreteOperationCastFrame(operationContextVariable, operation.OperationType);

            var apiOperationContextSource =
                new ApiOperationContextVariableSource(operationContextVariable, castFrame.CastOperationVariable);

            var context = new MiddlewareBuilderContext(
                executeMethod,
                operation,
                model,
                services,
                instanceFrameProvider,
                isNested);

            // For the base Exception type we will add, as the first step, logging to the exception sinks. This frame DOES NOT
            // include a return frame, as we add that after all the other middleware builders have had chance to potentially add
            // more frames to perform other operations on unknown Exception
            context.RegisterUnhandledExceptionHandler(typeof(Exception), e => new Frame[]
            {
                // Exceptions do not escape from a pipeline because we always convert to a result type
                new PushExceptionToActivityFrame(e, false),
            });

            executeMethod.Sources.Add(apiOperationContextSource);
            executeMethod.Sources.Add(dependencyInjectionVariableSource);

            foreach (var source in options.GenerationRules.VariableSources)
            {
                executeMethod.Sources.Add(source);
            }

            var startActivityFrame = ActivityFrame.Start(ActivityKind.Internal, operation.Name + (isNested ? "NestedPipeline" : "Pipeline"));
            executeMethod.Frames.Add(startActivityFrame);

            executeMethod.Frames.Add(castFrame);
            executeMethod.Frames.Add(new ErrorHandlerFrame(context));
            executeMethod.Frames.Add(new BlankLineFrame());

            foreach (var behaviour in options.MiddlewareBuilders)
            {
                if (isNested && !behaviour.SupportsNestedExecution)
                {
                    continue;
                }

                if (behaviour.Matches(operation))
                {
                    executeMethod.Frames.Add(new CommentFrame(behaviour.GetType().Name));

                    behaviour.Build(context);

                    executeMethod.Frames.Add(new BlankLineFrame());
                }
            }

            // For the base Exception type we will add, as a last frame, a return of an OperationResult.
            context.RegisterUnhandledExceptionHandler(typeof(Exception), e => new[]
            {
                new ReturnFrame(new Variable(
                    typeof(UnhandledExceptionOperationResult),
                    $"new {typeof(UnhandledExceptionOperationResult).FullNameInCode()}({e})")),
            });
        }

        /// <summary>
        /// A frame that will output the code to create a variable of the "proper" operation type, meaning that
        /// we do not need to constantly perform a cast on <see cref="ApiOperationContext.Operation" /> whenever we need
        /// the concrete type.
        /// </summary>
        private class ConcreteOperationCastFrame : SyncFrame
        {
            private readonly Argument _operationContextVariable;
            private readonly Variable _operationVariable;

            public ConcreteOperationCastFrame(Argument operationContextVariable, Type operationType)
            {
                this._operationContextVariable = operationContextVariable;

                this._operationVariable = new Variable(operationType, this);
            }

            public Variable CastOperationVariable => this._operationVariable;

            protected override void Generate(
                IMethodVariables variables,
                GeneratedMethod method,
                IMethodSourceWriter writer,
                Action next)
            {
                writer.WriteLine(
                    $"var {this._operationVariable} = ({this._operationVariable.VariableType.FullNameInCode()}) {this._operationContextVariable.GetProperty(nameof(ApiOperationContext.Operation))};");

                next();
            }
        }
    }
}
