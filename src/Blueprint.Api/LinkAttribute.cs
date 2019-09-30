using System;

namespace Blueprint.Api
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class LinkAttribute : Attribute
    {
        public LinkAttribute(Type resourceType, string url)
        {
            ResourceType = resourceType;
            Url = url;
        }

        protected LinkAttribute()
        {
        }

        public Type ResourceType { get; }

        public string Url { get; }

        public string Rel { get; set; }
    }
}
