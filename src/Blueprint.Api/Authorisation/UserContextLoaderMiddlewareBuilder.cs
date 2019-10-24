using System.Security;
using System.Security.Claims;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;

namespace Blueprint.Api.Authorisation
{
    public class UserContextLoaderMiddlewareBuilder : IMiddlewareBuilder
    {
        private const string AccessDeniedExceptionMessage = "Access denied. Anonymous access is not allowed.";

        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            if (context.Descriptor.AnonymousAccessAllowed)
            {
                // TODO: Should we be loading user auth context even if anon access allowed as some may still use if available?

                // Generates
                //
                // context.UserAuthorisationContext = AnonymousUserAuthorisationContext.Instance;

                // When we allow anonymous just inject the AnonymousUserAuthorisationContext at all times (is this right?)

                context.AppendFrames(
                    new VariableSetterFrame(
                            context.VariableFromContext<IUserAuthorisationContext>(),
                            Variable.StaticFrom<AnonymousUserAuthorisationContext>(nameof(AnonymousUserAuthorisationContext.Instance))));
            }
            else
            {
                // Generates:
                //
                // if (context.ClaimsIdentity == null)
                // {
                //     throw new SecurityException("Access denied. Anonymous access is not allowed.");
                // }
                //
                // var userSecurityContext = await this.userSecurityContextFactory.CreateContextAsync(context.ClaimsIdentity);
                // if (userSecurityContext == null)
                // {
                //     throw new SecurityException("Access denied. Anonymous access is not allowed.");
                // }
                //
                //  context.UserAuthorisationContext = userSecurityContext;
                var claimsIdentityVariable = context.VariableFromContext<ClaimsIdentity>();

                var userSecurityContextFactoryCreator = context.VariableFromContainer<IUserAuthorisationContextFactory>();
                var createContextCall = userSecurityContextFactoryCreator.Method(f => f.CreateContextAsync(null));
                createContextCall.TrySetArgument(claimsIdentityVariable);

                context.AppendFrames(
                    new IfNullBlock(claimsIdentityVariable)
                    {
                        new ThrowExceptionFrame<SecurityException>(AccessDeniedExceptionMessage),
                    });

                context.AppendFrames(
                    userSecurityContextFactoryCreator,
                    createContextCall);

                context.AppendFrames(
                    new IfNullBlock(createContextCall.ReturnVariable)
                    {
                        new ThrowExceptionFrame<SecurityException>(AccessDeniedExceptionMessage),
                    });

                context.AppendFrames(
                    new VariableSetterFrame(context.VariableFromContext<IUserAuthorisationContext>(), createContextCall.ReturnVariable));
            }
        }
    }
}
