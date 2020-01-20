using System.Threading.Tasks;
using Blueprint.Api.Authorisation;

namespace Blueprint.Api
{
    /// <summary>
    /// A state based link checker is used to enable the exclusion of operation links based on
    /// the state of the resource for which the link is for, for example removing a 'Deactivate' link from
    /// an already deactivated entity.
    /// </summary>
    /// <typeparam name="TOperation">The type of operation this checker is for.</typeparam>
    /// <typeparam name="TResource">The resource that is represented by the operation.</typeparam>
    public abstract class StateBasedLinkChecker<TOperation, TResource> : IApiAuthoriser where TOperation : IApiOperation
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Task<ExecutionAllowed> StateCheckFailed = Task.FromResult(ExecutionAllowed.No("Resource in incorrect state", "Cannot perform operation at this time.", ExecutionAllowedFailureType.Authorisation));

        public bool AppliesTo(ApiOperationDescriptor descriptor)
        {
            return descriptor.OperationType == typeof(TOperation);
        }

        public Task<ExecutionAllowed> CanExecuteOperationAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, IApiOperation operation)
        {
            return ExecutionAllowed.YesTask;
        }

        public Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            if (resource is TResource r && !IsLinkAvailableForOperation(operationContext, r))
            {
                return StateCheckFailed;
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
}
