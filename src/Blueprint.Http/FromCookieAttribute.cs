using System;

namespace Blueprint.Http
{
    /// <summary>
    /// Indicates that an API operation property should be loaded from cookies in the HTTP request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FromCookieAttribute : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FromCookieAttribute" /> class.
        /// </summary>
        public FromCookieAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="FromCookieAttribute" /> class with
        /// a cookie name override.
        /// </summary>
        /// <param name="name">The name of the cookie to look for, instead of using the property name.</param>
        public FromCookieAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the cookie, if different to the property name.
        /// </summary>
        public string Name { get; }
    }
}
