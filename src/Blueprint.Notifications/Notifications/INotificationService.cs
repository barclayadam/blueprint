namespace Blueprint.Notifications.Notifications
{
    /// <summary>
    /// Notification service that can send out any notifications and perform the necessary logging and tracking.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends out a notification, using all information within the given request context.
        /// </summary>
        /// <param name="templatePath">The template path.</param>
        /// <param name="options">The options used to send this notification.</param>
        void SendNotification(string templatePath, NotificationOptions options);
    }
}