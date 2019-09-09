namespace Blueprint.Core.Auditing
{
    /// <summary>
    /// Defines the information used to audit actions.
    /// </summary>
    public struct AuditItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditItem"/> class.
        /// </summary>
        /// <param name="correlationId">The context of the request.</param>
        /// <param name="wasSuccessful">True if the action was successful.</param>
        /// <param name="resultMessage">The message returned.</param>
        /// <param name="username">The username of the user who actioned the event.</param>
        /// <param name="details">The object that was passed as the action.</param>
        public AuditItem(string correlationId, bool wasSuccessful, string resultMessage, string username, object details)
        {
            CorrelationId = correlationId;
            ResultMessage = resultMessage;
            Username = username;
            Details = details;
            WasSuccessful = wasSuccessful;
        }

        /// <summary>
        /// Gets the object that was passed as the action, denormalised.
        /// </summary>
        public object Details { get; private set; }
        
        /// <summary>
        /// Gets the context of the request.
        /// </summary>
        public string CorrelationId { get; private set; }

        /// <summary>
        /// Gets the result of the action.
        /// </summary>
        public string ResultMessage { get; private set; }

        /// <summary>
        /// Gets the username of the actioning user.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the action was successful.
        /// </summary>
        public bool WasSuccessful { get; private set; }
    }
}