namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// Determines how the result of as method will be disposed, if
    /// at all.
    /// </summary>
    public enum DisposalMode
    {
        /// <summary>
        /// The result will be wrapped in a using block to dispose
        /// pf the result.
        /// </summary>
        UsingBlock,

        /// <summary>
        /// The result will not be disposed automatically.
        /// </summary>
        None,
    }
}
