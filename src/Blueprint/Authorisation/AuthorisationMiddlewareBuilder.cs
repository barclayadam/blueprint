using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Authorisation
{
    public class AuthorisationMiddlewareBuilder : IMiddlewareBuilder
    {
        private const string AccessDeniedExceptionMessage = "Access denied. Anonymous access is not allowed.";

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool SupportsNestedExecution => true;

        // ReSharper disable once MemberCanBePrivate.Global
        public static async Task EnforceAsync(IApiAuthoriser authoriser, ApiOperationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<AuthorisationMiddlewareBuilder>>();
            var result = await authoriser.CanExecuteOperationAsync(context, context.Descriptor, context.Operation);

            if (result.IsAllowed == false)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    // Operation executions failures should be less common (checked by app to avoid) so we can log the failure
                    logger.LogInformation(
                        "Permission check failed. reason={0} authoriser={1}",
                        result.Reason,
                        authoriser.GetType().Name);
                }

                if (result.FailureType == ExecutionAllowedFailureType.Authorisation)
                {
                    throw new ForbiddenException(result);
                }

                throw new SecurityException(result.Message);
            }
        }

        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.AnonymousAccessAllowed == false;
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var checkFrames = new List<Frame>();

            // Generates:
            //
            // if (!context.SkipAuthorisation)
            // {
            //     if (context.UserAuthorisationContext == null)
            //     {
            //         throw new SecurityException("Access denied. Anonymous access is not allowed.");
            //     }
            //
            //     foreach (authorisers) { await a.EnforceAsync(); }
            // }

            var authorisationContextVariable = context.FindVariable<IUserAuthorisationContext>();

            checkFrames.Add(
                new IfBlock($"{authorisationContextVariable} == null")
                {
                    new ThrowExceptionFrame<SecurityException>(AccessDeniedExceptionMessage),
                });

            foreach (var checker in context.ServiceProvider.GetServices<IApiAuthoriser>())
            {
                if (checker.AppliesTo(context.Descriptor))
                {
                    var getInstanceVariable = context.VariableFromContainer(checker.GetType());
                    var methodCall = new MethodCall(typeof(AuthorisationMiddlewareBuilder), nameof(EnforceAsync));

                    // HACK: We cannot set just by variable type as compiler fails with index out of range (believe this
                    // is because the declared type is IApiAuthoriser but variable is subtype)
                    methodCall.TrySetArgument("authoriser", getInstanceVariable.InstanceVariable);

                    checkFrames.Add(getInstanceVariable);
                    checkFrames.Add(methodCall);
                }
            }

            // We only run authorisation checks if SkipAuthorisation is false, which it will be by default
            context.AppendFrames(
                new IfBlock(
                    $"{context.FindVariable<ApiOperationContext>()}.{nameof(ApiOperationContext.SkipAuthorisation)} == false",
                    checkFrames.ToArray()));

            context.RegisterFinallyFrames(new SetApmUserDetailsFrame());
        }

        private class SetApmUserDetailsFrame : SyncFrame
        {
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));
                var spanVariable = apiOperationContextVariable.GetProperty(nameof(ApiOperationContext.ApmSpan));

                // It is possible that no span exists
                writer.If($"{spanVariable} != null");

                // We set user data as tags. Individual APM tools may wish to react accordingly to these tags if there is some specific
                // location user details need to be stored.
                writer.WriteLine($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");

                writer.If($"userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.WriteLine($"{spanVariable}.SetTag(\"user.id\", userContext.{nameof(IUserAuthorisationContext.Id)});");
                writer.WriteLine($"{spanVariable}.SetTag(\"user.account_id\", userContext.{nameof(IUserAuthorisationContext.AccountId)});");
                writer.FinishBlock();

                writer.FinishBlock();
            }
        }
    }
}
