using System;
using System.Net.Mail;
using Blueprint;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Notifications
{
    /// <summary>
    /// Sends an email message using the built-in <see cref="System.Net.Mail.SmtpClient" /> class,
    /// taking all configuration from the standard settings in app.config or web.config,
    /// as documented within the .NET framework.
    /// </summary>
    public class SmtpClientEmailSender : IEmailSender
    {
        private readonly ILogger<SmtpClientEmailSender> logger;
        private readonly SmtpClient smtpClient;

        public SmtpClientEmailSender(ILogger<SmtpClientEmailSender> logger, SmtpClient smtpClient)
        {
            this.logger = logger;
            this.smtpClient = smtpClient;
        }

        /// <summary>
        /// Sends the specified mail message using the built-in SmtpClient, constructing a new
        /// client which takes its configuration values from the current application's XML configuration
        /// file.
        /// </summary>
        /// <remarks>
        /// This method will not handle the disposing of the MailMessage provided, it is the
        /// responsibility of the client of this method to do so.
        /// </remarks>
        /// <param name="message">The message which is to be sent.</param>
        /// <exception cref="EmailSendingException">If a problem occurs attempting
        /// to send the message.</exception>
        public void Send(MailMessage message)
        {
            logger.LogDebug("Attempting to send email. to={0} from={1}", message.To, message.From);

            try
            {
                DoSend(message);

                logger.LogDebug("Successfully sent email. to={0} from={1}", message.To, message.From);
            }
            catch (InvalidOperationException e)
            {
                logger.LogWarning("Failed to send email. to={0} from={1}", message.To, message.From);

                object[] args = new[] {e.Message};
                throw new EmailSendingException(string.Format("An error has occurred attempting to send email ({0}).", args), e);
            }
            catch (SmtpException e)
            {
                logger.LogWarning("Failed to send email. to={0} from={1}", message.To, message.From);

                object[] args = new[] {e.Message};
                throw new EmailSendingException(string.Format("An error has occurred attempting to send email ({0}).", args), e);
            }
        }

        protected virtual void DoSend(MailMessage message)
        {
            Guard.NotNull(nameof(message), message);

            smtpClient.Send(message);
        }
    }
}
