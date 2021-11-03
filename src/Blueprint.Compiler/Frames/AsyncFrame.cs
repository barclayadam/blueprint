namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="Frame" /> that has async code.
    /// </summary>
    public abstract class AsyncFrame : Frame
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AsyncFrame" /> class.
        /// </summary>
        protected AsyncFrame()
            : base(true)
        {
        }
    }
}
