using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware that will populate the API operation using all registered
    /// <see cref="IMessagePopulationSource" />s.
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
            var sources = context.ServiceProvider
                .GetServices<IMessagePopulationSource>().OrderBy(s => s.Priority)
                .ToList();

            var allOwned = new List<PropertyInfo>();
            var ownedBySources = new Dictionary<IMessagePopulationSource, IEnumerable<PropertyInfo>>();

            foreach (var s in sources)
            {
                var ownedBySource = s.GetOwnedProperties(context.Model, context.Descriptor).ToList();

                ownedBySources[s] = ownedBySource;

                foreach (var p in ownedBySource)
                {
                    if (allOwned.Contains(p))
                    {
                        throw new InvalidOperationException(
                            $"Property {p.DeclaringType.Name}.{p.Name} has been marked as owned by multiple sources. A property can only be owned by a single source.");
                    }

                    allOwned.Add(p);
                }
            }

            foreach (var s in sources)
            {
                s.Build(allOwned, ownedBySources[s], context);
            }
        }
    }
}
