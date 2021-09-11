using System.Collections.Generic;

namespace Blueprint
{
    /// <summary>
    /// An interface placed on resources to indicate that it contains children resources, such that parts
    /// of the system such as link generation can process and add links to these children resources as if
    /// they were top-level.
    /// </summary>
    public interface IApiResourceCollection
    {
        /// <summary>
        /// Gets an enumerable over this resource's child resources.
        /// </summary>
        /// <returns>An enumerable of children.</returns>
        IEnumerable<object> GetEnumerable();
    }
}
