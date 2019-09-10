using System;
using Blueprint.Core.Authorisation;

namespace Blueprint.Api
{
    /// <inheritdoc />
    public class AnonymousUserAuthorisationContext : IUserAuthorisationContext
    {
        /// <summary>
        /// The single instance of <see cref="AnonymousUserAuthorisationContext"/> that can be used to indicate
        /// there is no current user.
        /// </summary>
        public static readonly AnonymousUserAuthorisationContext Instance = new AnonymousUserAuthorisationContext();

        private AnonymousUserAuthorisationContext()
        {
        }

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public bool IsActive => false;

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool IsAnonymous => true;

        /// <summary>
        /// Does nothing as no metadata to populate, given this is an anonymous user.
        /// </summary>
        /// <param name="add"></param>
        public void PopulateMetadata(Action<string, object> add)
        {
        }

        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        public string Name { get; }
    }
}
