using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blueprint.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    public class CodeGennedExecutor : IApiOperationExecutor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly GeneratedAssembly assembly;
        private readonly Dictionary<Type, Func<Type>> operationTypeToPipelineType;

        internal CodeGennedExecutor(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            GeneratedAssembly assembly,
            Dictionary<Type, Func<Type>> operationTypeToPipelineType)
        {
            DataModel = dataModel;

            this.serviceProvider = serviceProvider;
            this.assembly = assembly;
            this.operationTypeToPipelineType = operationTypeToPipelineType;
        }

        /// <summary>
        /// Gets the <see cref="ApiDataModel" /> that was used to create this executor, which will indicate what operations
        /// can be executed.
        /// </summary>
        public ApiDataModel DataModel { get; }

        /// <summary>
        /// Gets all of the code that was used to generate this executor.
        /// </summary>
        /// <returns>The code used to create all executors.</returns>
        public string WhatCodeDidIGenerate()
        {
            var builder = new StringBuilder();

            foreach (var type in assembly.GeneratedTypes)
            {
                builder.AppendLine(type.SourceCode);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <paramref name="operationType" />.
        /// </summary>
        /// <param name="operationType">The operation type to get source code for.</param>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor(Type operationType)
        {
            var generatedExecutorType = operationTypeToPipelineType[operationType]();
            var assemblyGeneratedType = assembly.GeneratedTypes.Single(t => t.CompiledType == generatedExecutorType);

            return assemblyGeneratedType.SourceCode;
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The operation type to get source code for.</typeparam>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor<T>() where T : IApiOperation
        {
            return WhatCodeDidIGenerateFor(typeof(T));
        }

        /// <inheritdoc />
        public Task<OperationResult> ExecuteAsync(ApiOperationContext context)
        {
            var pipelineType = operationTypeToPipelineType[context.Descriptor.OperationType]();
            var pipeline = (IOperationExecutorPipeline) ActivatorUtilities.CreateInstance(context.ServiceProvider, pipelineType);

            return context.IsNested ?
                pipeline.ExecuteNestedAsync(context) :
                pipeline.ExecuteAsync(context);
        }

        /// <inheritdoc />
        public async Task<OperationResult> ExecuteWithNewScopeAsync<T>(T operation) where T : IApiOperation
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var apiOperationContext = DataModel.CreateOperationContext(serviceScope.ServiceProvider, operation);

                return await ExecuteAsync(apiOperationContext);
            }
        }
    }
}
