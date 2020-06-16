namespace Blueprint.Core.Apm
{
    /// <summary>
    /// The span type as created in <see cref="IApmTool.Start" />.
    /// </summary>
    /// <remarks>
    /// Note that some tooling, such as OpenTracing, have no separation of span types.
    /// </remarks>
    public enum SpanType
    {
        /// <summary>
        /// This is a transaction, which is usually started at the edges of an application as a request
        /// comes in (for example an incoming HTTP request starts a transaction, or a dequeue to process
        /// a background message starts a transaction).
        /// </summary>
        Transaction,

        /// <summary>
        /// Spans belong to <see cref="Transaction" />s, representing individual parts of a transaction such
        /// as the sending of a HTTP call, the queuing of a message or database calls.
        /// </summary>
        Span,
    }
}
