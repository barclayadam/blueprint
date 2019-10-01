namespace Blueprint.Api
{
    /// <summary>
    /// Identifies a command that can be executed within the system to read data from it, the
    /// write side of a CQRS system.
    /// </summary>
    public interface ICommand : IApiOperation
    {
    }
}
