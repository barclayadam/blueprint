using System;
using Blueprint.Authorisation;

namespace Blueprint;

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
    /// Gets a value indicating whether the user is active, which always returns <c>false</c>.
    /// </summary>
    /// <returns><c>false</c>.</returns>
    public bool IsActive => false;

    /// <summary>
    /// Gets a value indicating whether the user is anonymous, which always returns <c>true</c>.
    /// </summary>
    /// <returns><c>true</c>.</returns>
    public bool IsAnonymous => true;

    /// <summary>
    /// Gets the Id of the user, always returns <c>null</c>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the account id of the user, always returns <c>null</c>.
    /// </summary>
    public string AccountId { get; }

    /// <summary>
    /// Gets the email of the user, always returns <c>null</c>.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the name of the user, always returns <c>null</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Does nothing as no metadata to populate, given this is an anonymous user.
    /// </summary>
    /// <param name="add">The method to call to add metadata.</param>
    public void PopulateMetadata(Action<string, object> add)
    {
    }
}