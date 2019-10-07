namespace Blueprint.Notifications.Handlers
{
    /// <summary>
    /// Options for <see cref="TemplatedEmailHandler" />.
    /// </summary>
    public class TemplatedEmailHandlerOptions
    {
        /// <summary>
        /// Gets or sets a modifier pattern for the recipient, which is used to modify the outgoing sender
        /// which can be useful for testing purposes.
        /// </summary>
        /// <remarks>
        /// The modified can be any valid email address with the following placeholders that will be replaced at
        /// runtime:
        ///
        /// 1. <code>{email</code> - The email being sent to, passed in as-is
        /// 2. <code>{safe-email}</code> - A "safe" email address that could be used in the local part as @ and + have been replaced
        ///                                by # and _ respectively.
        /// </remarks>
        public string RecipientModifier { get; set; }

        /// <summary>
        /// Gets or sets the default sender's email address.
        /// </summary>
        public string Sender { get; set; }
    }
}
