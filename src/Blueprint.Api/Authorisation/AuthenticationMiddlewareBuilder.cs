using System.Security.Claims;
using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Authorisation
{
    public class AuthenticationMiddlewareBuilder : IMiddlewareBuilder
    {
        public bool Matches(ApiOperationDescriptor operation)
        {
            // TODO: Should we be loading user auth context even if anon access allowed as some may still use if available?
            return operation.AnonymousAccessAllowed == false;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            /* Generates:
             if (context.ClaimsIdentity == null)
             {
                 var claimsIdentity = GetInstance<IClaimsIdentityProvider>.Get(context);
                 context.ClaimsIdentity = claimsIdentity;
             }
            */
            var claimsIdentityVariable = context.VariableFromContext<ClaimsIdentity>();
            var getClaimsIdentityProvider = context.VariableFromContainer<IClaimsIdentityProvider>();
            var getClaimsIdentity = MethodCall.For<IClaimsIdentityProvider>(p => p.Get(null));

            context.AppendFrames(
                new IfNullBlock(claimsIdentityVariable)
                {
                    getClaimsIdentityProvider,
                    getClaimsIdentity,
                    new VariableSetterFrame(claimsIdentityVariable, getClaimsIdentity.ReturnVariable),
                });
        }
    }
}
