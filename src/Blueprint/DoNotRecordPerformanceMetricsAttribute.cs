using System;

namespace Blueprint
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotRecordPerformanceMetricsAttribute : Attribute
    {
    }
}
