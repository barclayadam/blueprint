using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using Blueprint.Core.Utilities;

using NLog;

namespace Blueprint.Core.Notifications
{
    /// <summary>
    /// The repository class for Embedded Resource Notification templates.
    /// </summary>
    public class EmbeddedResourceNotificationRepository : INotificationRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly XmlSerializer EmailTemplateSerializer = new XmlSerializer(typeof(EmailTemplate));

        /// <summary>
        /// Gets the notification templates that are associated with a given named notification.
        /// </summary>
        /// <param name="name">The name of the notification.</param>
        /// <returns>The list of templates that represent notifications to be sent out for the named notification, or <c>null</c> to represent a non existent notification.</returns>
        public IEnumerable<INotificationTemplate> GetTemplates(string name)
        {
            var notificationContent = GetNotificationTemplate(name);

            if (notificationContent == null)
            {
                throw new InvalidOperationException($"Could not find embedded template at {name}");
            }

            if (notificationContent.Layout != null)
            {
                var layoutName = notificationContent.Layout;

                // We need to convert the referene to the layout from the stored template to an actual
                // layout.
                notificationContent.Layout = GetNotificationEmbeddedResourceContent(layoutName);

                if (notificationContent.Layout == null)
                {
                    throw new InvalidOperationException($"Could not find embedded resource layout at {layoutName}");
                }
            }

            Log.Debug("Embedded Resource Notification '{0}' has been found.", name);

            return new[] { notificationContent };
        }

        /// <summary>
        /// Gets the notification template.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns a notification email template.</returns>
        private EmailTemplate GetNotificationTemplate(string name)
        {
            var content = GetNotificationEmbeddedResourceContent(name);

            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                return (EmailTemplate)EmailTemplateSerializer.Deserialize(memStream);
            }
        }

        /// <summary>
        /// Gets the content of the notification embedded resource.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns the content if successful else returns an empty string.</returns>
        private static string GetNotificationEmbeddedResourceContent(string name)
        {
            return Assembly.GetExecutingAssembly().GetEmbeddedResourceAsString(name);
        }
    }
}