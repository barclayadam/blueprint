namespace Blueprint.Middleware
{
    /// <summary>
    /// Determines how handlers should handle the return value from method calls.
    /// </summary>
    public enum ExecutorReturnType
    {
        /// <summary>
        /// Indicates that the handler should NOT return and assign a variable from the handler
        /// execution.
        /// </summary>
        NoReturn,

        /// <summary>
        /// Indicates that the handler MUST return a variable that will be available in the source.
        /// </summary>
        Return,
    }
}
