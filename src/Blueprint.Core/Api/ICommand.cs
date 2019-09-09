using Blueprint.Core.ThirdParty;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// Identifies a command that can be executed within the system to read data from it, the
    /// write side of a CQRS system.
    /// </summary>
    [UsedImplicitly]
    public interface ICommand : IApiOperation
    {
    }
}