using System;

namespace Blueprint
{
    /// <summary>
    /// Contains information about the Blueprint environment, including whether we are
    /// servicing a precompilation request
    /// </summary>
    public static class BlueprintEnvironment
    {
        /// <summary>
        /// Indicates whether we are precompiling the Blueprint Pipeline DLL for the currently
        /// executing application, set via the BUILD_BLUEPRINT_API environment variable.
        /// </summary>
        public static readonly bool IsPrecompiling = Environment.GetEnvironmentVariable("BUILD_BLUEPRINT_API") == "true";
    }
}
