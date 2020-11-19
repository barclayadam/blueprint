using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace Blueprint.Notifications
{
    /// <summary>
    /// An <see cref="IEmailSender" /> that performs no real sending of any email messages, will only log
    /// the message using a configured <see cref="ILogger{LoggingOnlyEmailSender}" />.
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
        private readonly ILogger<LoggingOnlyEmailSender> _logger;

        public LoggingOnlyEmailSender(ILogger<LoggingOnlyEmailSender> logger)
        {
            this._logger = logger;
        }

        public void Send(MailMessage message)
        {
            this._logger.LogInformation(
                     "Sending mail to '{0}', from '{1}'. {2} attachments. Subject is '{3}'. Body is '{4}'.",
                     message.From,
                     message.To,
                     message.Attachments.Count,
                     message.Subject,
                     message.Body);
        }
    }
}
