using System;

namespace Blueprint;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class LinkAttribute : Attribute
{
    public LinkAttribute(Type resourceType, string routePattern)
    {
        this.ResourceType = resourceType;
        this.RoutePattern = routePattern;
    }

    protected LinkAttribute()
    {
    }

    public Type ResourceType { get; }

    public string RoutePattern { get; }

    public string Rel { get; set; }
}