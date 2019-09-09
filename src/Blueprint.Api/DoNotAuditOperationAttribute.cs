using System;

namespace Blueprint.Api
{
    // TODO: Merge with DoNotAudit attribute in Blueprint.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotAuditOperationAttribute : Attribute
    {
    }
}