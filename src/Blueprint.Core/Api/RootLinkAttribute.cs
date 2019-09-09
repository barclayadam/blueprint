using System;

namespace Blueprint.Core.Api
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class RootLinkAttribute : LinkAttribute
    {
        public RootLinkAttribute(string resourceUrl)
            : base(null, resourceUrl)
        {
        }
    }
}
