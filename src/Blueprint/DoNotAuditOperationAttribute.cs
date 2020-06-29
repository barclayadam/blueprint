using System;

namespace Blueprint
{
    // TODO: Merge with DoNotAudit attribute in Blueprint.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotAuditOperationAttribute : Attribute
    {
    }
}
