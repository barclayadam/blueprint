using System.Collections.Generic;
using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Notifications.Handlers;
using Microsoft.Extensions.Logging;

namespace Blueprint.Notifications
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
        private readonly INotificationRepository notificationRepository;
        private readonly IEnumerable<INotificationHandler> handlers;
        private readonly ILogger<LoggingOnlyEmailSender> logger;

        /// <summary>
        /// Initializes a new instance of the NotificationService class.
        /// </summary>
        /// <param name="notificationRepository">The notification repository.</param>
        /// <param name="handlers">All registered notification handlers.</param>
        /// <param name="logger">The logger to use.</param>
        public NotificationService(INotificationRepository notificationRepository, IEnumerable<INotificationHandler> handlers, ILogger<LoggingOnlyEmailSender> logger)
        {
            Guard.NotNull(nameof(notificationRepository), notificationRepository);
            Guard.NotNull(nameof(handlers), handlers);
            Guard.NotNull(nameof(logger), logger);

            this.notificationRepository = notificationRepository;
            this.handlers = handlers;
            this.logger = logger;
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

            logger.LogInformation("Processing notification template_path={0}", templatePath);

            foreach (var template in templates)
            {
                foreach (var handler in handlers)
                {
                    ProcessUsingHandler(template, options, handler);
                }
            }
        }

        private void ProcessUsingHandler(INotificationTemplate notificationTemplate, NotificationOptions options, INotificationHandler handler)
        {
            var handlerName = handler.GetType().Name;

            logger.LogDebug("Processing notification. handler={0}", handlerName);

            handler.Handle(notificationTemplate, options);
        }
    }
}
