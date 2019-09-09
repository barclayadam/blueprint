namespace Blueprint.Api
{
    /// <summary>
    /// A result that is used when no handle could be found for a given API operation. In general usage this should 
    /// never be used, as there should be no client using an <see cref="ApiOperationExecutor"/> with an API operation
    /// that has not had a handle registered.
    /// </summary>
    public sealed class OperationHandlerNotFoundResult
    {
        public static readonly OperationHandlerNotFoundResult Instance = new OperationHandlerNotFoundResult();
    }
}