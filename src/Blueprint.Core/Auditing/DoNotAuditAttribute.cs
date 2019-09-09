using System;

namespace Blueprint.Core.Auditing
{
    /// <summary>
    /// An attribute that can applied to a property of a message (i.e. <see cref="IApiOperation" />) to 
    /// indicate that it should not be stored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DoNotAuditAttribute : Attribute
    {
    }
}