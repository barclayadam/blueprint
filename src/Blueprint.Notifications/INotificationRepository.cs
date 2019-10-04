using System.Collections.Generic;

namespace Blueprint.Notifications
{
    /// <summary>
    /// Provides the means to get <see cref="INotificationTemplate"/>s from a persistent store, typically
    /// being the CMS being used (e.g. Ektron) by the system.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Gets the notification templates that are associated with a given named notification.
        /// </summary>
        /// <param name="name">The name of the notification.</param>
        /// <returns>The list of templates that represent notifications to be sent out for the
        /// named notification, or <c>null</c> to represent a non-existant notification..</returns>
        IEnumerable<INotificationTemplate> GetTemplates(string name);
    }
}