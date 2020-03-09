using System;

namespace Blueprint.Api.Configuration
{
    public interface IOperationScannerConvention
    {
        void Apply(ApiOperationDescriptor descriptor);

        bool ShouldInclude(Type operationType);
    }
}
