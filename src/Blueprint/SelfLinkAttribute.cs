using System;

namespace Blueprint
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class SelfLinkAttribute : LinkAttribute
    {
        public SelfLinkAttribute(Type resourceType, string resourceUrl)
            : base(resourceType, resourceUrl)
        {
            this.Rel = "self";
        }
    }
}
