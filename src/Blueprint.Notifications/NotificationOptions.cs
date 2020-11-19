using System.Collections.Generic;
using Blueprint.Notifications.Templates;

namespace Blueprint.Notifications
{
    /// <summary>
    /// A number of notification options that can (and in some cases, must) be used to
    /// set various options used by notification handlers when sending a notification.
    /// </summary>
    public class NotificationOptions
    {
        /// <summary>
        /// Initializes a new instance of the NotificationOptions class.
        /// </summary>
        public NotificationOptions()
        {
            this.Attachments = new List<NotificationAttachment>();
        }

        /// <summary>
        /// Gets or sets the 'from' email address that should be set instead of
        /// the one set in the template.
        /// </summary>
        /// <remarks>
        /// If no from email address is set here then the one defined in the template
        /// will be used, and an exception will be thrown if one has not been
        /// specified at all.
        /// </remarks>
        public string FromEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the 'to' email address, the destination of the email.
        /// </summary>
        /// <remarks>
        /// This value will be set in <b>addition</b> to any values set in
        /// the email template loaded, with an exception being thrown if neither
        /// this or the to address in the template has been set.
        /// </remarks>
        public string ToEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a set of values that should be injected into the values used to
        /// populate notifications (including from and to email addresses, this is not
        /// limited to just the main body).
        /// </summary>
        public TemplateValues TemplateValues { get; set; }

        /// <summary>
        /// Gets any file attachments that should be sent with this notification
        /// if the specified handler (e.g. email) can handle it.
        /// </summary>
        public List<NotificationAttachment> Attachments { get; }
    }
}
