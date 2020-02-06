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
            var model = options.Model;

            // We have multiple ways in which we work with generated assemblies, depending on context:
            //
            //  - We are writing unit tests which create many pipelines (within Blueprint itself). Here we
            //    would want to use in-memory compilation and assembly loading only
            //
            //  - We have deployed an app using generated code. We want to use pre-compiled DLLs loaded as
            //    part of the usual loading process. This is done by creating an assembly and PDB that is
            //    deployed with the application and loaded below (see step 1)
            //
            //  - We are in development. Here we wish to generate and load a new DLL on application startup and
            //    store in the temp folder of the machine. This means the DLL is _not_ loaded as normal part
            //    of .NET process and therefore we can (re)create at will on startup without worrying about
            //    the existence of an existing DLL

            // 1. Try and find an already loaded assembly
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == options.Rules.AssemblyName)
                {
                    // The assembly exists in the current domain, therefore it has either already been generated in this
                    // process OR it has previously been compiled and loaded as part of normal assembly loading (pre-compiled
                    // as part of dotnet publish)

                    logger.LogInformation("Assembly {AssemblyName} already exists, using to create executor.", options.Rules.AssemblyName);

                    var typeToCreationMappings = new Dictionary<Type, Func<Type>>();
                    var exportedTypes = assembly.GetExportedTypes();

                    foreach (var operation in options.Model.Operations)
                    {
                        var typeName = GetTypeName(operation);
                        var operationType = exportedTypes.SingleOrDefault(t => t.FullName == typeName);

                        if (operationType == null)
                        {
                            throw new InvalidOperationException(
                                $"The assembly {options.Rules.AssemblyName} loaded in the current domain is NOT valid as it is missing executor pipeline " +
                                $"{typeName} for operation {operation.Name}");
                        }

                        typeToCreationMappings.Add(operation.OperationType, () => operationType);
                    }

                    return new CodeGennedExecutor(
                        serviceProvider,
                        model,
                        null,
                        typeToCreationMappings);
                }
            }

            // 2. We DO NOT have any existing DLLs. In that case we are going to generate the source code using our configured
            // middlewares and then hand off to AssemblyGenerator to compile and load the assembly (which may be in-memory, stored
            // to a temp folder or stored to the project output folder)
            logger.LogInformation("Building CodeGennedExecutor for {0} operations", options.Model.Operations.Count());

            using (var serviceScope = serviceProvider.CreateScope())
            {
                foreach (var middleware in options.Middlewares)
                {
                    Use(middleware);
                }

                var typeToCreationMappings = new Dictionary<Type, Func<Type>>();

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

                    var typeName = GetTypeName(operation);

                    var pipelineExecutorType = assembly.AddType(
                        typeName,
                        typeof(IOperationExecutorPipeline));

                    // We need to set up a LoggerVariable once, to be shared between methods
                    var executorLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeName);
                    pipelineExecutorType.AllInjectedFields.Add(new LoggerVariable(typeName, executorLogger));

                    var executeMethod = pipelineExecutorType.MethodFor(nameof(IOperationExecutorPipeline.ExecuteAsync));
                    var executeNestedMethod = pipelineExecutorType.MethodFor(nameof(IOperationExecutorPipeline.ExecuteNestedAsync));

                    Generate(serviceProvider, executeMethod, operation, model, serviceScope, false);
                    Generate(serviceProvider, executeNestedMethod, operation, model, serviceScope, true);

                    typeToCreationMappings.Add(
                        operation.OperationType,
                        () => pipelineExecutorType.CompiledType);
                }

                logger.LogInformation("Compiling {0} pipeline executors", typeToCreationMappings.Count);
                assembly.CompileAll(serviceProvider.GetRequiredService<IAssemblyGenerator>());
                logger.LogInformation("Done compiling {0} pipeline executors", typeToCreationMappings.Count);

                return new CodeGennedExecutor(
                    serviceProvider,
                    model,
                    assembly,
                    typeToCreationMappings);
            }
        }

        private static string GetTypeName(ApiOperationDescriptor operation)
        {
            // Replace + with _ to enable nested operation classes to compile successfully
            return operation.OperationType.FullName.Replace("+", "_") + "ExecutorPipeline";
        }

        private void Generate(
            IServiceProvider serviceProvider,
            GeneratedMethod executeMethod,
            ApiOperationDescriptor operation,
            ApiDataModel model,
            IServiceScope serviceScope,
            bool isNested)
        {
            var operationContextVariable = executeMethod.Arguments[0];

            var instanceFrameProvider = serviceProvider.GetRequiredService<InstanceFrameProvider>();
            var dependencyInjectionVariableSource = new DependencyInjectionVariableSource(executeMethod, instanceFrameProvider);

            var castFrame = new ConcreteOperationCastFrame(operationContextVariable, operation.OperationType);

            var apiOperationContextSource =
                new ApiOperationContextVariableSource(operationContextVariable, castFrame.CastOperationVariable);

            var context = new MiddlewareBuilderContext(
                executeMethod,
                apiOperationContextSource,
                operation,
                model,
                serviceScope.ServiceProvider,
                instanceFrameProvider,
                isNested);

            // For the base Exception type we will add, as the first step, logging to the exception sinks. This frame DOES NOT
            // include a return frame, as we add that after all the other middleware builders have had chance to potentially add
            // more frames to perform other operations on unknown Exception
            context.RegisterUnhandledExceptionHandler(typeof(Exception), e => new[]
            {
                new PushToErrorLoggerExceptionCatchFrame(context, e),
            });

            executeMethod.Sources.Add(apiOperationContextSource);
            executeMethod.Sources.Add(dependencyInjectionVariableSource);

            executeMethod.Frames.Add(castFrame);
            executeMethod.Frames.Add(new ErrorHandlerFrame(context));
            executeMethod.Frames.Add(new BlankLineFrame());

            foreach (var behaviour in builders)
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
                new ReturnFrame(new Variable(typeof(UnhandledExceptionOperationResult), $"new {typeof(UnhandledExceptionOperationResult).FullNameInCode()}({e})")),
            });
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

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.Write(
                    $"var {operationVariable} = ({operationVariable.VariableType.FullNameInCode()}) {operationContextVariable.GetProperty(nameof(ApiOperationContext.Operation))};");

                next();
            }
        }
    }
}
