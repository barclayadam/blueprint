namespace Blueprint.Apm
{
    /// <summary>
    /// A set of span kinds to be used when starting new <see cref="IApmSpan" /> through a registered
    /// <see cref="IApmTool" />.
    /// </summary>
    public static class SpanKinds
    {
        /// <summary>
        /// A client span represents the execution of a piece of code as the client, i.e. a dependency
        /// on an external system that will produce a result such as a SQL or HTTP call.
        /// </summary>
        public const string Client = "client";

        /// <summary>
        /// A server span represents the processing of a client's request (i.e. a HTTP call), typically
        /// the other end of <see cref="Client" /> spans.
        /// </summary>
        public const string Server = "server";

        /// <summary>
        /// A consumer span represents the execution of a piece of code in response to a queue message.
        /// </summary>
        public const string Consumer = "consumer";

        /// <summary>
        /// A producer span represents the placing of messages on to a queue, with the other end being
        /// of type <see cref="Consumer" />.
        /// </summary>
        public const string Producer = "producer";

        /// <summary>
        /// An internal span has no external communication and is instead designer to track portions of
        /// a transaction that may take a period of time and should be marked as an individual period of
        /// time for more fine grained tracking purposes.
        /// </summary>
        public const string Internal = "internal";
    }
}
