using System;

namespace Blueprint.Auditing
{
    /// <summary>
    /// An attribute that can applied to a property of a message (i.e. an operation definition) to
    /// indicate that it should not be stored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DoNotAuditAttribute : Attribute
    {
    }
}
