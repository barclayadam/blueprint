using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Type, IOperationExecutorPipeline> operationTypeToPipelineType;

        internal CodeGennedExecutor(IServiceProvider serviceProvider, ApiDataModel dataModel, GeneratedAssembly assembly, Dictionary<Type, IOperationExecutorPipeline> operationTypeToPipelineType)
        {
            DataModel = dataModel;

            this.serviceProvider = serviceProvider;
            this.assembly = assembly;
            this.operationTypeToPipelineType = operationTypeToPipelineType;
        }

        public ApiDataModel DataModel { get; }

        public string WhatCodeDidIGenerate()
        {
            var builder = new StringBuilder();

            foreach (var type in assembly.GeneratedTypes)
            {
                builder.AppendLine(type.SourceCode);
            }

            return builder.ToString();
        }

        public Task<OperationResult> ExecuteAsync(ApiOperationContext context)
        {
            return operationTypeToPipelineType[context.Descriptor.OperationType].ExecuteAsync(context);
        }

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
