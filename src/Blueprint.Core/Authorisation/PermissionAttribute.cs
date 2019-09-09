using System;

namespace Blueprint.Core.Authorisation
{
    /// <summary>
    /// When applied to a message indicates that the user executing the command must have
    /// a given permission, a permission that is 'system-wide' and as such is not associated
    /// with any particular resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class PermissionAttribute : ClaimRequiredAttribute
    {
        private readonly string permission;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAttribute"/> class. 
        /// Initializes a new instance of the PermissionForAttribute.
        /// </summary>
        /// <param name="permission">The named permission that is required.</param>
        public PermissionAttribute(string permission)
            : base(ClaimTypes.Permission, "*", permission)
        {
            this.permission = permission;
        }

        /// <summary>
        /// Gets the permission this attribute is representing as being required.
        /// </summary>
        public string Permission
        {
            get
            {
                return permission;
            }
        }
    }
}