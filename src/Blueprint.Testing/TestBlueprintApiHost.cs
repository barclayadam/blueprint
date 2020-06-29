using Blueprint.Api.Configuration;
using Blueprint.Configuration;

namespace Blueprint.Testing
{
    /// <summary>
    /// An <see cref="IBlueprintApiHost" /> that can be used in integration tests when creating an
    /// API when no environment-specific host is needed.
    /// </summary>
    public class TestBlueprintApiHost : IBlueprintApiHost
    {
    }
}
