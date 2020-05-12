using System.Collections.Generic;
using System.Reflection;
using Blueprint.Compiler.Frames;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A message population source provides the means to populate a message / operation, for example from
    /// HTTP bodies (JSON, Form, XML etc.), ambient data like the user or tenant information, or route data.
    /// </summary>
    public interface IMessagePopulationSource
    {
        /// <summary>
        /// Gets the priority for this source, which can be used to give higher precedence to certain sources
        /// so they cannot be overriden by more generic sources (i.e. values form the route should NOT be overriden
        /// from a JSON HTTP body source, therefore the Route source should have a <b>high</b> priority).
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets a list of properties that this source "owns", which may be empty.
        /// </summary>
        /// <remarks>
        /// A source can mark a property as "owned" to indicate to the message population middleware
        /// that the property can and should only be set by this source.
        /// </remarks>
        /// <remarks>
        /// If all properties are considered "owned" then it would be possible for certain sources to be
        /// completely omitted as they should only be populating non-owned properties (i.e, a JSON body source
        /// could be omitted if ALL properties are considered owned by other sources).
        /// </remarks>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>A list of owned properties.</returns>
        IEnumerable<PropertyInfo> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor);

        /// <summary>
        /// Builds this source's body, adding the required <see cref="Frame" />s to the pipeline's body.
        /// </summary>
        /// <param name="allOwnedProperties">The properties that are considered to be "owned", used to potentially
        ///     exclude this source's output.</param>
        /// <param name="ownedBySource">The set of properties from <seealso cref="GetOwnedProperties" />.</param>
        /// <param name="context">The builder context.</param>
        void Build(
            IReadOnlyCollection<PropertyInfo> allOwnedProperties,
            IEnumerable<PropertyInfo> ownedBySource,
            MiddlewareBuilderContext context);
    }
}
