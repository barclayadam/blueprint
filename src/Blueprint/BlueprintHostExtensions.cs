// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extensions to <see cref="IHost" /> that provide better integration
/// for running Blueprint API hosts.
/// </summary>
public static class BlueprintHostExtensions
{
    /// <summary>
    /// Runs a Blueprint API.
    /// </summary>
    /// <param name="host">The host to run.</param>
    public static void RunBlueprint(this IHost host)
    {
        host.Run();
    }
}