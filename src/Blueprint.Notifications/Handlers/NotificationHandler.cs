namespace Blueprint.Notifications.Handlers
{
    public abstract class NotificationHandler<TTemplate> : INotificationHandler where TTemplate : INotificationTemplate
    {
        /// <summary>
        /// Handles a single notification template that has been loaded, along with
        /// customer details and other metadata associated.
        /// </summary>
        /// <param name="notificationTemplate">The notification template to be handled.</param>
        /// <param name="options">The notification options used in this request.</param>
        public void Handle(INotificationTemplate notificationTemplate, NotificationOptions options)
        {
            if (notificationTemplate is TTemplate)
            {
                this.InternalHandle((TTemplate)notificationTemplate, options);
            }
        }

        protected abstract void InternalHandle(TTemplate tTemplate, NotificationOptions options);
    }
}
