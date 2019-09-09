using System;

namespace Blueprint.Core.Api
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DoNotCompareAttribute : Attribute
    {
    }
}