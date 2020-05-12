using System.Security.Claims;
using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Authorisation
{
    public class AuthenticationMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public bool SupportsNestedExecution => false;

        /// <summary>
        /// Matches if the operation <see cref="ApiOperationDescriptor.AnonymousAccessAllowed" /> property
        /// is <c>false</c> (i.e. any authenticated operation will have this injected).
        /// </summary>
        /// <param name="operation">The operation to check for a match.</param>
        /// <returns>Whether this middleware matches.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            // TODO: Should we be loading user auth context even if anon access allowed as some may still use if available?
            return operation.AnonymousAccessAllowed == false;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            var getClaimsIdentityProvider = context.TryGetVariableFromContainer<IClaimsIdentityProvider>();

            // A claims identity provider may not be registered, we can skip generation altogether
            if (getClaimsIdentityProvider == null)
            {
                return;
            }

            /* Generates:
             if (context.ClaimsIdentity == null)
             {
                 var claimsIdentity = GetInstance<IClaimsIdentityProvider>.Get(context);
                 context.ClaimsIdentity = claimsIdentity;
             }
            */

            var claimsIdentityVariable = context.FindVariable<ClaimsIdentity>();
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
