namespace Blueprint.Core.Authorisation
{
    /// <summary>
    /// A resource key expander is used to take a resource key from a claim, and 'expand' it such that somebody
    /// who has a claim on a resource that is the parent of an item also intrinsically has that same claim on all children.
    /// </summary>
    public interface IResourceKeyExpander
    {
        /// <summary>
        /// Given a resource key will 'expand' it, turning it into a hierarchical version of the resource
        /// key (for example turning `User/1` into `Account/6/Team/5/User/1`), which will allow claims checking to
        /// have a claim for a resource key higher in the hierarchy apply to those lower (e.g. a permission claim
        /// for `Account/6` would grant that same permission claim to `User/1` in the previous example).
        /// </summary>
        /// <param name="resourceKey">The resource key to 'expand'.</param>
        /// <returns>The expanded key, or <c>null</c> if this expander does not handle the specified key.</returns>
        string Expand(string resourceKey);
    }
}