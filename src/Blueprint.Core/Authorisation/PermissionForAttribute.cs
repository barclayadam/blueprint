using System;

namespace Blueprint.Core.Authorisation
{
    /// <summary>
    /// When applied to a message indicates that the user executing the command must have
    /// a permission applied at the given resource key (or above if a hierarchy exists for
    /// the given resource).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class PermissionForAttribute : ClaimRequiredAttribute
    {
        private readonly string permission;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionForAttribute"/> class.
        /// Initializes a new instance of the PermissionForAttribute.
        /// </summary>
        /// <param name="permission">The named permission that is required.</param>
        /// <param name="resourceKeyTemplate">The resource key, which may contain templated variables (e.g. Site/{SiteId}).</param>
        public PermissionForAttribute(string permission, string resourceKeyTemplate)
            : base(ClaimTypes.Permission, resourceKeyTemplate, permission)
        {
            this.permission = permission;
        }

        /// <summary>
        /// Gets the permission this attribute is representing as being required.
        /// </summary>
        public string Permission => permission;
    }
}
