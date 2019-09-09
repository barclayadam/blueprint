using System.Net.Mail;

namespace Blueprint.Core.Notifications
{
    /// <summary>
    /// A class which is able to send emails.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email using this email sender, taking all information from the specified message.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is the responsibility of the subclasses to ensure that the configuration is valid to
        /// allow sending email and that the configuration required has been performed when
        /// constructing the object.
        /// </para>
        /// <para>
        /// This method will not handle the disposing of the MailMessage provided, it is the
        /// responsibility of the client of this method to do so.
        /// </para>
        /// </remarks>
        /// <param name="message">The message to be sent.</param>
        /// <exception cref="EmailSendingException">If a problem occurs attempting to send the message.</exception>
        void Send(MailMessage message);
    }
}