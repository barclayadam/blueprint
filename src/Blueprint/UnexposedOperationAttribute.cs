using System;

namespace Blueprint
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class UnexposedOperationAttribute : Attribute
    {
    }
}
