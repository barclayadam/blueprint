using System;

namespace Blueprint.Core.Api
{
    // TODO: Merge with DoNotAudit attribute in Blueprint.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotAuditOperationAttribute : Attribute
    {
    }
}