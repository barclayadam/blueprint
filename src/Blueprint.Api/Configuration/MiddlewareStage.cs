using System.Security.Claims;

namespace Blueprint.Api.Configuration
{
    public enum MiddlewareStage
    {
        /// <summary>
        /// This stage happens at the very start and is typically used to setup "global" frames like logging or APM integration.
        /// </summary>
        Setup,

        /// <summary>
        /// This stage is when the operation is populated, for example when we populate from the current HTTP request.
        /// </summary>
        Population,

        /// <summary>
        /// This stage is when we authenticate the user, which may mean simply loading the ambient <see cref="ClaimsIdentity" />
        /// from the HTTP request (i.e. integration with ASP.NET's existing auth providers)
        /// </summary>
        Authentication,

        /// <summary>
        /// This stage is when we authorise the user against the rules of the operation. At this point it is expected that the
        /// operation has been completely populated from both incoming request data (if HTTP) and other sources, for example
        /// from the user loading during authentication.
        /// </summary>
        Authorisation,

        /// <summary>
        /// This stage validates the operation, using libraries such as DataAnnotations, to ensure the data is as expected to
        /// successfully process the operation.
        /// </summary>
        Validation,

        /// <summary>
        /// The execution is the calling of an <see cref="IApiOperationHandler{T}"/> that can process the operation, the actual unique
        /// logic per operation.
        /// </summary>
        Execution,

        /// <summary>
        /// This stage happens after the actual execution, useful for things like auditing.
        /// </summary>
        PostExecution,

        /// <summary>
        /// This stage happens last, just before returning the result and is intended to clean up any resources as necessary (note that this
        /// <strong>does not</strong> imply the middleware frames would be executed when an exception is thrown, this is for the "happy path".
        /// </summary>
        Cleanup,
    }
}
