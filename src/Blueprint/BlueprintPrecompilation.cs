using System;
using Microsoft.Extensions.Hosting;

namespace Blueprint
{
    /// <summary>
    /// Supporting class for handling Blueprint pre-compilation.
    /// </summary>
    public static class BlueprintPrecompilation
    {
        /// <summary>
        /// Indicates whether we are precompiling the Blueprint Pipeline DLL for the currently
        /// executing application, set via the BUILD_BLUEPRINT_API environment variable.
        /// </summary>
        public static readonly bool IsPrecompiling = Environment.GetEnvironmentVariable("BUILD_BLUEPRINT_API") == "true";

        /// <summary>
        /// Runs a Blueprint API and handles precompiling requests.
        /// </summary>
        /// <remarks>
        /// In the normal case of not requiring precompilation <see cref="HostingAbstractionsHostExtensions.Run" />
        /// will be called. If precompilation is required then the DLL will be compiled and the
        /// application will exit.
        /// </remarks>
        /// <param name="host">The host to run.</param>
        public static void RunBlueprint(this IHost host)
        {
            // If we are precompiling then we just need to grab an instance of IApiOperationExecutor
            // which will force the compilation of the DLL and then exit the application.
            if (IsPrecompiling)
            {
                host.Services.GetService(typeof(IApiOperationExecutor));
            }
            else
            {
                // We are not precompiling so run as usual.
                host.Run();
            }
        }
    }
}
