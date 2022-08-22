using System;

namespace Blueprint.Authorisation;

public interface IUserAuthorisationContext
{
    /// <summary>
    /// Gets a value indicating whether the user can be considered 'active', which means that
    /// have access to the site (even if not a particular section due to their privileges).
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets a value indicating whether the user is using the site without being logged in.
    /// </summary>
    bool IsAnonymous { get; }

    string Id { get; }

    string AccountId { get; }

    string Email { get; }

    string Name { get; }

    /// <summary>
    /// Populates metadata about this user using the given action which adds data to the destination
    /// store.
    /// </summary>
    /// <param name="add">The action to call to add data about this user to.</param>
    void PopulateMetadata(Action<string, object> add);
}