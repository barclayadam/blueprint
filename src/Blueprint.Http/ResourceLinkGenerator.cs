using System.Threading.Tasks;

namespace Blueprint.Http
{
    /// <summary>
    /// A small helper base class implementing <see cref="IResourceLinkGenerator" /> that will check for the correct type of
    /// resource and pass that on to a typed method.
    /// </summary>
    /// <typeparam name="T">The type of resource the class handles.</typeparam>
    public abstract class ResourceLinkGenerator<T> : IResourceLinkGenerator where T : class, ILinkableResource
    {
        /// <inheritdoc />
        public ValueTask AddLinksAsync(IApiLinkGenerator apiLinkGenerator, ApiOperationContext context, ILinkableResource linkableResource)
        {
            if (!(linkableResource is T asTypedResource))
            {
                return default;
            }

            return this.AddLinksAsync(apiLinkGenerator, context, asTypedResource);
        }

        /// <inheritdoc cref="IResourceLinkGenerator.AddLinksAsync" />
        protected abstract ValueTask AddLinksAsync(IApiLinkGenerator apiLinkGenerator, ApiOperationContext context, T linkableResource);
    }
}
