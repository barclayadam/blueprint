using System.Net.Mail;
using NLog;

namespace Blueprint.Notifications.Notifications
{
    /// <summary>
    /// An <see cref="IEmailSender" /> that performs no real sending of any email messages, will only log
    /// the message using NLog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is particularly useful during development, or in environments that emails are not required
    /// such as UI tests where the emails would be lost and not useful in any case.
    /// </para>
    /// <para>
    /// The message details will be logged at an <c>Info</c> level using the <c>Notifications</c> logger, 
    /// including from and to addresses, the count of attachments, the subject and body.
    /// </para>
    /// </remarks>
    public class LoggingOnlyEmailSender : IEmailSender
    {
        private static readonly Logger Log = LogManager.GetLogger("Blueprint.Notifications");

        public void Send(MailMessage message)
        {
            Log.Info(
                     "Sending mail to '{0}', from '{1}'. {2} attachments. Subject is '{3}'. Body is '{4}'.",
                     message.From, 
                     message.To,
                     message.Attachments.Count,
                     message.Subject,
                     message.Body);
        }
    }
}
