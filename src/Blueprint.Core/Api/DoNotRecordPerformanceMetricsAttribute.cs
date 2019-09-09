using System;

namespace Blueprint.Core.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotRecordPerformanceMetricsAttribute : Attribute
    {
    }
}