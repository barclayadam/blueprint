namespace Blueprint.Notifications.Handlers
{
    /// <summary>
    /// A handler of a notification.
    /// </summary>
    public interface INotificationHandler
    {
        /// <summary>
        /// Handles a single notification template that has been loaded, along with
        /// customer details and other metadata associated.
        /// </summary>
        /// <param name="notificationTemplate">The notification template to be handled.</param>
        /// <param name="options">The notification options used in this request.</param>
        void Handle(INotificationTemplate notificationTemplate, NotificationOptions options);
    }
}