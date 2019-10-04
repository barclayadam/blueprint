using System;

namespace Blueprint.Core.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryKeyAttribute : Attribute
    {
        public DictionaryKeyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
