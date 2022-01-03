using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint
{
    public class CodeGennedExecutor : IApiOperationExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Func<Type>> _operationTypeToPipelineType;
        private readonly Dictionary<Type, string> _sourceCodeMappings;

        internal CodeGennedExecutor(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            GeneratedAssembly assembly,
            Dictionary<Type, Func<Type>> operationTypeToPipelineType)
        {
            this.DataModel = dataModel;

            this._serviceProvider = serviceProvider;
            this._operationTypeToPipelineType = operationTypeToPipelineType;

            if (assembly != null)
            {
                // We only need to store the source code for each type. We need to discard the GeneratedAssembly otherwise
                // it holds on to a lot of memory
                this._sourceCodeMappings = new Dictionary<Type, string>();

                foreach (var t in assembly.GeneratedTypes)
                {
                    this._sourceCodeMappings[t.CompiledType] = t.GeneratedSourceCode;
                }
            }
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
            if (this._sourceCodeMappings == null)
            {
                throw new InvalidOperationException("Cannot get source code if the pipelines were precompiled");
            }

            var builder = new StringBuilder();

            foreach (var type in this._sourceCodeMappings)
            {
                builder.AppendLine(type.Value);
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
            if (this._sourceCodeMappings == null)
            {
                throw new InvalidOperationException("Cannot get source code if the pipelines were precompiled");
            }

            var generatedExecutorType = this._operationTypeToPipelineType[operationType]();
            return this._sourceCodeMappings[generatedExecutorType];
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The operation type to get source code for.</typeparam>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor<T>()
        {
            return this.WhatCodeDidIGenerateFor(typeof(T));
        }

        /// <inheritdoc />
        public Task<OperationResult> ExecuteAsync(ApiOperationContext context)
        {
            var pipelineType = this._operationTypeToPipelineType[context.Descriptor.OperationType]();
            var pipeline = (IOperationExecutorPipeline)ActivatorUtilities.CreateInstance(context.ServiceProvider, pipelineType);

            return context.IsNested ?
                pipeline.ExecuteNestedAsync(context) :
                pipeline.ExecuteAsync(context);
        }

        /// <inheritdoc />
        public async Task<OperationResult> ExecuteWithNewScopeAsync(object operation, CancellationToken token = default)
        {
            using var serviceScope = this._serviceProvider.CreateScope();
            var apiOperationContext = this.DataModel.CreateOperationContext(serviceScope.ServiceProvider, operation, token);

            return await this.ExecuteAsync(apiOperationContext);
        }
    }
}
