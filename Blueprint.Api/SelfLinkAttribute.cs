using System;

namespace Blueprint.Api
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class SelfLinkAttribute : LinkAttribute
    {
        public SelfLinkAttribute(Type resourceType, string resourceUrl)
            : base(resourceType, resourceUrl)
        {
            Rel = "self";
        }
    }
}