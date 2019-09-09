using System;

namespace Blueprint.Core.Authorisation
{
    /// <summary>
    /// An attribute that decorates a resource to indicate that no authorisation checks are
    /// required as anonymous users can access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
    }
}