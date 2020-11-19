using System;

namespace Blueprint
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class LinkAttribute : Attribute
    {
        public LinkAttribute(Type resourceType, string url)
        {
            this.ResourceType = resourceType;
            this.Url = url;
        }

        protected LinkAttribute()
        {
        }

        public Type ResourceType { get; }

        public string Url { get; }

        public string Rel { get; set; }
    }
}
