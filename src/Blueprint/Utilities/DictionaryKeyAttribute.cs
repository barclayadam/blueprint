using System;

namespace Blueprint.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryKeyAttribute : Attribute
    {
        public DictionaryKeyAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
