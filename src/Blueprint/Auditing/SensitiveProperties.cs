using System;
using System.Reflection;

using Blueprint.Utilities;

namespace Blueprint.Auditing
{
    /// <summary>
    /// Helper to deal with sensitive properties defined for an operation, allowing them to be excluded from
    /// logs etc.
    /// </summary>
    public static class SensitiveProperties
    {
        /// <summary>
        /// Determines whether the given member should be considered sensitive and therefore not be
        /// logged outside of the application to error logs, audit trails etc.
        /// </summary>
        /// <remarks>
        /// A property can be marked explicitly as sensitive by adding the <see cref="SensitiveAttribute" /> or
        /// <see cref="DoNotAuditAttribute "/> attributes. Any property that has the text "password" in it's name
        /// will also be considered sensitive (case-insensitive check).
        /// </remarks>
        /// <param name="p">The property to check.</param>
        /// <returns>Whether or not the property represents sensitive data.</returns>
        public static bool IsSensitive(MemberInfo p)
        {
            return p.HasAttribute<DoNotAuditAttribute>(true) ||
                   p.HasAttribute<SensitiveAttribute>(true) ||
                   p.Name.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
