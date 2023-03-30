using System.Security.Claims;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Authorisation;

public class UserContextLoaderMiddlewareBuilder : IMiddlewareBuilder
{
    /// <summary>
    /// Returns <c>false</c>.
    /// </summary>
    public bool SupportsNestedExecution => false;

    public bool Matches(ApiOperationDescriptor operation)
    {
        return true;
    }

    public void Build(MiddlewareBuilderContext context)
    {
        // Generates:
        //
        // if (context.ClaimsIdentity?.IsAuthenticated == true)
        // {
        //     var userSecurityContext = await this.userSecurityContextFactory.CreateContextAsync(context.ClaimsIdentity);
        //     context.UserAuthorisationContext = userSecurityContext;
        // }
        var claimsIdentityVariable = context.FindVariable<ClaimsIdentity>();

        var userSecurityContextFactoryCreator = context.VariableFromContainer<IUserAuthorisationContextFactory>();
        var createContextCall = userSecurityContextFactoryCreator.Method(f => f.CreateContextAsync(null));
        createContextCall.TrySetArgument(claimsIdentityVariable);

        context.AppendFrames(
            new IfBlock($"{claimsIdentityVariable}?.{nameof(ClaimsIdentity.IsAuthenticated)} == true")
            {
                userSecurityContextFactoryCreator,
                createContextCall,
                new VariableSetterFrame(context.FindVariable<IUserAuthorisationContext>(), createContextCall.ReturnVariable),
            });
    }
}
