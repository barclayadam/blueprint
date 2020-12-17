using System;

namespace Blueprint.Http
{
    /// <summary>
    /// Indicates that an API operation property should be loaded from the body, which changes the
    /// behaviour of the population middleware to NOT populate any other properties from the
    /// body of the HTTP request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FromBodyAttribute : Attribute
    {
    }
}
