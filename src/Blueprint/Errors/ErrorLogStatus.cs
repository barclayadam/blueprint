namespace Blueprint.Errors
{
    public enum ErrorLogStatus
    {
        /// <summary>
        /// Indicates the exception was ignored, because it is one that is of
        /// no importance to ops and is expected in the course of the application.
        /// </summary>
        Ignored,

        /// <summary>
        /// Indicates the exception was recorded, that it was unexpected, and that the
        /// handler of this exception should continue to treat this as an unhandled
        /// exception (e.g. it may cause a task to fail and be retried).
        /// </summary>
        Recorded,
    }
}
