using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Blueprint.Auditing;
using Blueprint.Compiler.Frames;
using JetBrains.Annotations;

namespace Blueprint.Middleware;

/// <summary>
/// A middleware that performs auditing, recording the details of each execution of an API
/// operation by delegating work to an <see cref="IAuditor"/> to store details of executed
/// operations, who actioned the request, correlation IDs, error messages and more.
/// </summary>
public class AuditMiddleware : IMiddlewareBuilder
{
    /// <summary>
    /// Returns <c>false</c>.
    /// </summary>
    public bool SupportsNestedExecution => false;

    [UsedImplicitly]
    public static void WriteSuccess(IAuditor auditor, ApiOperationContext context)
    {
        auditor.Write(new AuditItem(
            Activity.Current?.Id ?? "no-activity-id",
            true,
            "Success",
            GetUserId(context),
            context.Operation));
    }

    [UsedImplicitly]
    public static void WriteFailure(IAuditor auditor, ApiOperationContext context, Exception e)
    {
        auditor.Write(new AuditItem(
            Activity.Current?.Id ?? "no-activity-id",
            false,
            e.Message,
            GetUserId(context),
            context.Operation));
    }

    /// <summary>
    /// Returns <c>true</c> if <see cref="ApiOperationDescriptor.ShouldAudit" /> is <c>true</c> AND the
    /// <see cref="ApiOperationDescriptor.IsCommand" /> is <c>true</c>.
    /// </summary>
    /// <param name="operation">The operation descriptor check.</param>
    /// <returns>Whether the execution of the given operation should be audited.</returns>
    public bool Matches(ApiOperationDescriptor operation)
    {
        return operation.ShouldAudit && operation.IsCommand;
    }

    public void Build(MiddlewareBuilderContext context)
    {
        context.AppendFrames(new MethodCall(this.GetType(), nameof(WriteSuccess)));

        context.RegisterUnhandledExceptionHandler(typeof(Exception), (e) =>
        {
            var methodCall = new MethodCall(this.GetType(), nameof(WriteFailure));
            methodCall.TrySetArgument(e);

            return new[]
            {
                methodCall,
            };
        });
    }

    private static string GetUserId(ApiOperationContext context)
    {
        return context.ClaimsIdentity == null
            ? "Anonymous"
            : context.ClaimsIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Sid || c.Type == "sub")?.Value ?? "Unknown";
    }
}