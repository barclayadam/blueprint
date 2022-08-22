using System.Linq;
using System.Threading.Tasks;

namespace Blueprint.Authorisation;

public class MustBeAuthenticatedApiAuthoriser : IApiAuthoriser
{
    private static readonly Task<ExecutionAllowed> _userUnauthenticated = Task.FromResult(ExecutionAllowed.No("Operation does not have AllowAnonymous attribute. User is unauthenticated", "Please log in", ExecutionAllowedFailureType.Authentication));
    private static readonly Task<ExecutionAllowed> _userInactive = Task.FromResult(ExecutionAllowed.No("Operation does not have AllowAnonymous attribute. User is unauthenticated", "Please log in", ExecutionAllowedFailureType.Authentication));

    public bool AppliesTo(ApiOperationDescriptor descriptor)
    {
        return descriptor.TypeAttributes.OfType<MustBeAuthenticatedAttribute>().SingleOrDefault() != null;
    }

    public Task<ExecutionAllowed> CanExecuteOperationAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object operation)
    {
        return IsAuthorisedAsync(operationContext);
    }

    public Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
    {
        return IsAuthorisedAsync(operationContext);
    }

    private static Task<ExecutionAllowed> IsAuthorisedAsync(ApiOperationContext operationContext)
    {
        if (operationContext.UserAuthorisationContext == null)
        {
            return _userUnauthenticated;
        }

        if (!operationContext.UserAuthorisationContext.IsActive)
        {
            return _userInactive;
        }

        return ExecutionAllowed.YesTask;
    }
}