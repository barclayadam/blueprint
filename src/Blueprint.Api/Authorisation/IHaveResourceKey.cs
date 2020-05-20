namespace Blueprint.Api.Authorisation
{
    /// <summary>
    /// Marks a class of having a "resource key", as used in the <see cref="ClaimsRequiredApiAuthoriser" />.
    /// </summary>
    public interface IHaveResourceKey
    {
        /// <summary>
        /// The resource key of this resource.
        /// </summary>
        public string ResourceKey { get; }
    }
}
