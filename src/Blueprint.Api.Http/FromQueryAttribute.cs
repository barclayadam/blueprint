using System;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// Indicates that an API operation property should be loaded from the query string in the HTTP request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FromQueryAttribute : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FromQueryAttribute" /> class.
        /// </summary>
        public FromQueryAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="FromQueryAttribute" /> class with
        /// a query name override.
        /// </summary>
        /// <param name="name">The name of the query string to look for, instead of using the property name.</param>
        public FromQueryAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the query string, if different to the property name.
        /// </summary>
        public string Name { get; }
    }
}
