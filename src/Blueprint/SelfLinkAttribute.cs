using System;

namespace Blueprint;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class SelfLinkAttribute : LinkAttribute
{
    public SelfLinkAttribute(Type resourceType, string resourceRoutePattern)
        : base(resourceType, resourceRoutePattern)
    {
        this.Rel = "self";
    }
}