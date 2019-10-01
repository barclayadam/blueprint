namespace Blueprint.Api
{
    /// <summary>
    /// Identifies a query that can be executed within the system to read data from it, the
    /// read side of a CQRS system.
    /// </summary>
    public interface IQuery : IApiOperation
    {
    }
}
