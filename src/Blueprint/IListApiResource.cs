using System.Collections.Generic;

namespace Blueprint
{
    /// <summary>
    /// An <see cref="ApiResource" /> that contains other resources, used to enable other parts
    /// of the system, such as link generation, to process the child resources.
    /// </summary>
    public interface IListApiResource
    {
        /// <summary>
        /// Gets an enumerable over this resource's child resources.
        /// </summary>
        /// <returns>An enumerable of children.</returns>
        IEnumerable<object> GetEnumerable();
    }
}
