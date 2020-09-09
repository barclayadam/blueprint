using Blueprint;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Extensions to <see cref="IHost" /> that provide better integration
    /// for running Blueprint API hosts, including precompilation support.
    /// </summary>
    public static class BlueprintHostExtensions
    {
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
            if (BlueprintEnvironment.IsPrecompiling)
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
