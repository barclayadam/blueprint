using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint;

/// <summary>
/// A state based link checker is used to enable the exclusion of operation links based on
/// the state of the resource for which the link is for, for example removing a 'Deactivate' link from
/// an already deactivated entity.
/// </summary>
/// <typeparam name="TOperation">The type of operation this checker is for.</typeparam>
/// <typeparam name="TResource">The resource that is represented by the operation.</typeparam>
public abstract class StateBasedLinkChecker<TOperation, TResource> : IApiAuthoriser
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Task<ExecutionAllowed> _stateCheckFailed = Task.FromResult(ExecutionAllowed.No("Resource in incorrect state", "Cannot perform operation at this time.", ExecutionAllowedFailureType.Authorisation));

    public bool AppliesTo(ApiOperationDescriptor descriptor)
    {
        return descriptor.OperationType == typeof(TOperation);
    }

    /// <summary>
    /// Always returns <see cref="ExecutionAllowed.YesTask" /> as state-based checks do not apply to the execution of
    /// the operations, only link generation.
    /// </summary>
    /// <param name="operationContext">The operation context.</param>
    /// <param name="descriptor">The operation description.</param>
    /// <param name="operation">The operation.</param>
    /// <returns><see cref="ExecutionAllowed.YesTask" />.</returns>
    public Task<ExecutionAllowed> CanExecuteOperationAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object operation)
    {
        return ExecutionAllowed.YesTask;
    }

    public Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
    {
        if (resource is TResource r && !this.IsLinkAvailableForOperation(operationContext, r))
        {
            return _stateCheckFailed;
        }

        return ExecutionAllowed.YesTask;
    }

    /// <summary>
    /// Indicates whether a link is available for the the operation (as identified by the
    /// <typeparamref name="TOperation"></typeparamref> type parameter) given the specified resource and current state.
    /// </summary>
    /// <param name="operationContext">The context of the currently executing operation.</param>
    /// <param name="resource">The resource to check against.</param>
    /// <returns>Whether the operation link is available based on the current state of the resource.</returns>
    protected abstract bool IsLinkAvailableForOperation(ApiOperationContext operationContext, TResource resource);
}