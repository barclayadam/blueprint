namespace Blueprint.Authorisation
{
    /// <summary>
    /// Available claim types used within Blueprint.
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>
        /// A claim type that indicates a user has a permission (e.g. 'View Patient Records'), typically
        /// against some resource but may be against the 'system'.
        /// </summary>
        public const string Permission = "urn:claims/permission";

        /// <summary>
        /// A claim type that indicates a user has a role (e.g. 'Manager'), typically against some
        /// resource but may be against the 'system'.
        /// </summary>
        public const string Role = "urn:claims/role";
    }
}
