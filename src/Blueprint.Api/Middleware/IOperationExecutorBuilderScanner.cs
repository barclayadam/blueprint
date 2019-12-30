using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A scanner that can be used to search for a way of handling <see cref="IApiOperation"/>s that have been registered with an <see cref="ApiDataModel" />,
    /// being responsible for searching through referenced assemblies to find as many <see cref="IOperationExecutorBuilder"/> as it can (missing ones are handled
    /// by the coordinator, not the individual scanners).
    /// </summary>
    public interface IOperationExecutorBuilderScanner
    {
        /// <summary>
        /// Finds as many handlers for the given set of operations, returning any that match as corresponding <see cref="IOperationExecutorBuilder"/>s, optionally
        /// registering any required instances with the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The service collection that services can be registered for use at pipeline runtime.</param>
        /// <param name="operations">The operations to scan for builders for.</param>
        /// <returns>The list of found builders.</returns>
        IEnumerable<IOperationExecutorBuilder> FindHandlers(
            IServiceCollection services,
            IEnumerable<ApiOperationDescriptor> operations);
    }
}
