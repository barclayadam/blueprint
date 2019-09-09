using System;

namespace Blueprint.Api
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class UnexposedOperationAttribute : Attribute
    {
    }
}
