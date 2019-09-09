using System.Collections.Generic;
using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Notifications.Notifications.Handlers;
using NLog;

namespace Blueprint.Notifications.Notifications
{
    /// <summary>
    /// Notification service that can, given <see cref="NotificationOptions"/>,
    /// send out any notifications and perform the necessary logging and tracking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This notification service delegates all handling of the notifications and
    /// associated templates to registered <see cref="INotificationHandler"/>s that
    /// are passed in at construction time.
    /// </para>
    /// <para>
    /// This class' responsibility is only to log the incoming requests and to
    /// pass off the request to each interested handler.
    /// </para>
    /// </remarks>
    public class NotificationService : INotificationService
    {
        private static readonly Logger Log = LogManager.GetLogger("Blueprint.Notifications");

        private readonly INotificationRepository notificationRepository;
        private readonly IEnumerable<INotificationHandler> handlers;

        /// <summary>
        /// Initializes a new instance of the NotificationService class.
        /// </summary>
        /// <param name="notificationRepository">The notification repository.</param>
        /// <param name="handlers">All registered notification handlers.</param>
        public NotificationService(INotificationRepository notificationRepository, IEnumerable<INotificationHandler> handlers)
        {
            Guard.NotNull(nameof(notificationRepository), notificationRepository);
            Guard.NotNull(nameof(handlers), handlers);

            this.notificationRepository = notificationRepository;
            this.handlers = handlers;
        }

        /// <summary>
        /// Sends out a notification, using all information within the given request context.
        /// </summary>
        /// <param name="templatePath">The template path.</param>
        /// <param name="options">The options used to send this notification.</param>
        public void SendNotification(string templatePath, NotificationOptions options)
        {
            var templates = notificationRepository.GetTemplates(templatePath);

            if (templates == null)
            {
                throw new NotificationNotFoundException("Notification '{0}' could not be not found.".Fmt(templatePath));
            }

            Log.Info("Processing notification template_path={0}", templatePath);

            foreach (var template in templates)
            {
                foreach (var handler in handlers)
                {
                    ProcessUsingHandler(template, options, handler);
                }
            }
        }

        private static void ProcessUsingHandler(INotificationTemplate notificationTemplate, NotificationOptions options, INotificationHandler handler)
        {
            var handlerName = handler.GetType().Name;

            Log.Debug("Processing notification. handler={0}", handlerName);

            handler.Handle(notificationTemplate, options);
        }
    }
}
