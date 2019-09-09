using System;

namespace Blueprint.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotRecordPerformanceMetricsAttribute : Attribute
    {
    }
}