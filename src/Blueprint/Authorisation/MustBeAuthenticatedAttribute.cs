using System;

namespace Blueprint.Authorisation
{
    /// <summary>
    /// An attribute that should decorate a resource to indicate anyone who has been successfully
    /// authenticated by the system can access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MustBeAuthenticatedAttribute : Attribute
    {
    }
}
