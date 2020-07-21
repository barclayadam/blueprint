namespace Blueprint
{
    /// <summary>
    /// A marker interface that can be placed on an operation class to indicate what type of response
    /// it will generate.
    /// </summary>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    public interface IReturn<TResponse>
    {
    }
}
