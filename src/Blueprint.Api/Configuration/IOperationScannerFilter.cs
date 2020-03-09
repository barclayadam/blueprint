using System;

namespace Blueprint.Api.Configuration
{
    internal interface IOperationScannerFilter
    {
        bool ShouldInclude(Type operationType);
    }
}
