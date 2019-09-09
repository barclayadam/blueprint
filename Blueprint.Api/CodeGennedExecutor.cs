using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blueprint.Compiler;

namespace Blueprint.Api
{
    public class CodeGennedExecutor : IApiOperationExecutor
    {
        private readonly GeneratedAssembly assembly;
        private readonly Dictionary<Type, IOperationExecutorPipeline> operationTypeToPipelineType;

        public CodeGennedExecutor(ApiDataModel dataModel, GeneratedAssembly assembly, Dictionary<Type, IOperationExecutorPipeline> operationTypeToPipelineType)
        {
            DataModel = dataModel;
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

        public Task<OperationResult> Execute(ApiOperationContext ctx)
        {
            return operationTypeToPipelineType[ctx.Descriptor.OperationType].Execute(ctx);
        }
    }
}
