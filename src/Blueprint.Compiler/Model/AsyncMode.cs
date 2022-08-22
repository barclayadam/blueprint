using Blueprint.Compiler.Frames;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// Determines the async mode of a <see cref="GeneratedMethod" />, which influences code
    /// generation.
    /// </summary>
    public enum AsyncMode
    {
        /// <summary>
        /// The method uses async/await.
        /// </summary>
        AsyncTask,

        /// <summary>
        /// The method returns directly from the last <see cref="Frame" />, as no other frames
        /// are marked as async but method expects a Task return.
        /// </summary>
        ReturnFromLastNode,

        /// <summary>
        /// The method is not async.
        /// </summary>
        None,
    }
}
