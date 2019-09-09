namespace Blueprint.Core.Notifications
{
    /// <summary>
    /// An email template that will belong to a Notification, a template that
    /// is used to send an email to a customer.
    /// </summary>
    public class EmailTemplate : INotificationTemplate
    {
        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        public string Layout { get; set; }

        /// <summary>
        /// Gets the body of this email template, which will be passed through
        /// a template engine before being used.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the from email address of this email template.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets the email address to send this notification to (being an optional
        /// value that is applied in addition to any explicit value when sending a notification).
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Gets the subject of this email template, which will be passed 
        /// through a template engine before being used.
        /// </summary>
        public string Subject { get; set; }
    }
}