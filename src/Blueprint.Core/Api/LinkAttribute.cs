using System;

namespace Blueprint.Core.Api
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class LinkAttribute : Attribute
    {
        protected LinkAttribute()
        {
        }

        public LinkAttribute(Type resourceType, string url)
        {
            ResourceType = resourceType;
            Url = url;
        }

        public Type ResourceType { get; }

        public string Url { get; }

        public string Rel { get; set; }
    }
}