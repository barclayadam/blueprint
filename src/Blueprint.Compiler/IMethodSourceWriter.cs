using Blueprint.Compiler.Frames;

namespace Blueprint.Compiler
{
    /// <summary>
    /// A specialisation of <see cref="ISourceWriter" /> when writing out the body of a
    /// <see cref="GeneratedMethod" />.
    /// </summary>
    public interface IMethodSourceWriter : ISourceWriter
    {
        /// <summary>
        /// Writes the supplied <see cref="Frame" /> to this writer, assuming that the
        /// <see cref="Frame" /> is an independent one that exists only as a means of
        /// reusing existing frames.
        /// </summary>
        /// <param name="frame">The frame to write.</param>
        void Write(Frame frame);
    }
}
