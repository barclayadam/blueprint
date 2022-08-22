using System;

namespace Blueprint.Configuration;

internal interface IOperationScannerFilter
{
    bool ShouldInclude(Type operationType);
}