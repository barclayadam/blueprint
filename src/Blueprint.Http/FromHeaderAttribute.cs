using System;

namespace Blueprint.Http;

/// <summary>
/// Indicates that an API operation property should be loaded from headers in the HTTP request.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FromHeaderAttribute : Attribute
{
    /// <summary>
    /// Initialises a new instance of the <see cref="FromHeaderAttribute" /> class.
    /// </summary>
    public FromHeaderAttribute()
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="FromHeaderAttribute" /> class with
    /// a header name override.
    /// </summary>
    /// <param name="name">The name of the header to look for, instead of using the property name.</param>
    public FromHeaderAttribute(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// The name of the header, if different to the property name.
    /// </summary>
    public string Name { get; }
}