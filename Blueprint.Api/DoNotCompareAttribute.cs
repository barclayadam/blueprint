using System;

namespace Blueprint.Api
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DoNotCompareAttribute : Attribute
    {
    }
}