using System;

namespace Blueprint;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class RootLinkAttribute : LinkAttribute
{
    public RootLinkAttribute(string routePattern)
        : base(null, routePattern)
    {
    }
}