using System.Threading.Tasks;

namespace Blueprint.Authorisation;

public interface IApiAuthoriser
{
    /// <summary>
    /// Indicates whether or not this authoriser applies to the specified operation, used to narrow
    /// down which authorisers should be executed and checked for each operation.
    /// </summary>
    /// <param name="descriptor">The descriptor to check.</param>
    /// <returns>Whether this authoriser applies to the descriptor.</returns>
    bool AppliesTo(ApiOperationDescriptor descriptor);

    /// <summary>
    /// Indicates whether an operation can be executed given the user executing the operation, the descriptor
    /// of that operation and the instantiated and populated operation itself.
    /// </summary>
    /// <param name="operationContext">The operation requested to be executed.</param>
    /// <param name="descriptor">The descriptor of the operation.</param>
    /// <param name="operation">The populated operation.</param>
    /// <returns>Whether the operation link is available based on the current state of the resource.</returns>
    Task<ExecutionAllowed> CanExecuteOperationAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object operation);

    /// <summary>
    /// Indicates whether a link is available for the operation given the specified resource and
    /// current state.
    /// </summary>
    /// <param name="operationContext">The operation currently being executed, returning the given resource.</param>
    /// <param name="descriptor">The descriptor of the operation.</param>
    /// <param name="resource">The resource to check against.</param>
    /// <returns>Whether the operation link is available based on the current state of the resource.</returns>
    Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource);
}