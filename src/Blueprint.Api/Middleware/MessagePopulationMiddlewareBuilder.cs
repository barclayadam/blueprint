using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware that will populate the API operation that is being passed through with information
    /// from the <see cref="HttpRequestMessage" />.
    /// </summary>
    public class MessagePopulationMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public bool SupportsNestedExecution => false;

        /// <summary>
        /// Returns <c>true</c> if <paramref name="operation"/>.<see cref="ApiOperationDescriptor.OperationType"/> has any
        /// properties, <c>false</c> otherwise (as no properties == nothing to set).
        /// </summary>
        /// <param name="operation">The operation to check against.</param>
        /// <returns>Whether to apply this middleware.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.Properties.Any();
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            var sources = context.ServiceProvider.GetServices<IMessagePopulationSource>().OrderBy(s => s.Priority);
            var owned = new List<PropertyInfo>();

            foreach (var s in sources)
            {
                var ownedBySource = s.GetOwnedProperties(context);

                foreach (var p in ownedBySource)
                {
                    if (owned.Contains(p))
                    {
                        throw new InvalidOperationException(
                            $"Property {p.DeclaringType.Name}.{p.Name} has been marked as owned by multiple sources. A property can only be owned by a single source.");
                    }

                    owned.Add(p);
                }
            }

            foreach (var s in sources)
            {
                s.Build(owned, context);
            }
        }
    }
}
