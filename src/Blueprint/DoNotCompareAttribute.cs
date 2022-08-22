using System;

namespace Blueprint;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DoNotCompareAttribute : Attribute
{
}