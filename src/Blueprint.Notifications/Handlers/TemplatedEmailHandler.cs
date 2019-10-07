using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Notifications.Templates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint.Notifications.Handlers
{
    /// <summary>
    /// Handles notifications that have an EmailTemplate attached, merging the
    /// values into the templated attributes of the email before sending.
    /// </summary>
    public class TemplatedEmailHandler : NotificationHandler<EmailTemplate>
    {
        private readonly ITemplateFactory templateFactory;
        private readonly IEmailSender emailSender;
        private readonly ILogger<TemplatedEmailHandler> logger;
        private readonly IOptions<TemplatedEmailHandlerOptions> options;

        /// <summary>
        /// Initializes a new instance of the TemplatedEmailHandler class.
        /// </summary>
        /// <param name="templateFactory">The template factory.</param>
        /// <param name="emailSender">The email sender.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="options">The options that for this handler.</param>
        public TemplatedEmailHandler(ITemplateFactory templateFactory, IEmailSender emailSender, ILogger<TemplatedEmailHandler> logger, IOptions<TemplatedEmailHandlerOptions> options)
        {
            Guard.NotNull(nameof(templateFactory), templateFactory);
            Guard.NotNull(nameof(emailSender), emailSender);

            this.templateFactory = templateFactory;
            this.emailSender = emailSender;
            this.logger = logger;
            this.options = options;
        }

        protected override void InternalHandle(EmailTemplate emailTemplate, NotificationOptions options)
        {
            using (var message = CreateMessage(emailTemplate, options))
            {
                emailSender.Send(message);
            }
        }

        private static string UnescapeXmlCharacters(string text)
        {
            var unescapedText = text.Replace("&lt;", "<");
            unescapedText = unescapedText.Replace("&gt;", ">");
            unescapedText = unescapedText.Replace("&amp;", "&");
            unescapedText = unescapedText.Replace("&quot;", "\"");

            return unescapedText.Replace("&apos;", "'");
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This message is used in SendEmail which disposes of it.")]
        private MailMessage CreateMessage(EmailTemplate emailTemplate, NotificationOptions options)
        {
            var fromEmailAddress = emailTemplate.From;

            if (!string.IsNullOrEmpty(options.FromEmailAddress))
            {
                fromEmailAddress = options.FromEmailAddress;
            }

            if (string.IsNullOrEmpty(fromEmailAddress))
            {
                throw new InvalidOperationException("No 'from' email address has been specified for the email template.");
            }

            var message = new MailMessage
            {
                Subject = ApplyTemplate("Subject", emailTemplate.Subject, options),
                Body = ApplyTemplate("Body", emailTemplate.Body, options, emailTemplate.Layout),
                From = new MailAddress(ApplyTemplate("From", fromEmailAddress, options)),
                IsBodyHtml = true,
            };

            // If the from address does not have the same domain as our main sender address then we
            // will set an sender to ensure email validity checks (e.g. DKIM) are executed against our
            // controlled domain, otherwise we can use the from address as the sender
            var sender = new MailAddress(this.options.Value.Sender);
            message.Sender = sender.Host != message.From.Host ? sender : message.From;

            foreach (var attachment in options.Attachments)
            {
                message.Attachments.Add(attachment.ToMailAttachment());
            }

            if (!string.IsNullOrEmpty(options.ToEmailAddress))
            {
                message.To.Add(ModifyRecipient(ApplyTemplate("To-Options", options.ToEmailAddress, options)));
            }

            if (!string.IsNullOrEmpty(emailTemplate.To))
            {
                message.To.Add(ModifyRecipient(ApplyTemplate("To-Template", emailTemplate.To, options)));
            }

            if (message.To.Count == 0)
            {
                throw new InvalidOperationException(
                    "No 'to' email address has been specified for the email template.");
            }

            return message;
        }

        private string ModifyRecipient(string email)
        {
            if (options.Value.RecipientModifier == string.Empty)
            {
                return email;
            }

            if (string.IsNullOrEmpty(email))
            {
                return email;
            }

            // Always process the emails using the specified formatter. When not wanting to send real
            // emails we can forward to a catch-all email address and, if supported, include the {safe-email}
            // value which can be used as a local part of an email (i.e. ts+{safe-email}@gmail.com).
            //
            // Prod, with no changes to the email, should just specify a empty string as the config value to ensure
            // no modification happens

            // We parse using MailAddress first to handle emails such as "Timestamp Team <an@email.com>"
            var address = new MailAddress(email).Address;
            var safe = address.Replace("@", "#").Replace("+", "_");

            var modified = options.Value.RecipientModifier
                               .Replace("{email}", email)
                               .Replace("{safe-email}", safe);

            if (modified != address)
            {
                logger.LogDebug($"Modified email address for sending. original={address} replaced={modified}");
            }

            // If we have not replaced the email address return the original incoming, as that may be
            // formatted with sender name (i.e Recruitment Genius <jean@rg.com>)
            return modified;
        }

        private string ApplyTemplate(string propertyTemplateAppliedTo, string text, NotificationOptions options, string layout = null)
        {
            var templatesValues = options.TemplateValues;

            var baseContent = templateFactory
                .CreateTemplate(propertyTemplateAppliedTo, UnescapeXmlCharacters(text))
                .Merge(templatesValues);

            if (layout == null)
            {
                return baseContent;
            }

            return templateFactory
                .CreateTemplate(propertyTemplateAppliedTo + "Layout", layout)
                .Merge(new TemplateValues
                {
                    { "Body", baseContent },
                });
        }
    }
}
