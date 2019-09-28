using System;

namespace Blueprint.Core.Auditing
{
    /// <summary>
    /// An attribute that can applied to a property of a message (i.e. an API operation definition) to indicate
    /// that it should not be stored as it represents sensitive information (e.g. a password).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SensitiveAttribute : Attribute
    {
    }
}
