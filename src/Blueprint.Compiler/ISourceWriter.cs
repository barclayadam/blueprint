namespace Blueprint.Compiler
{
    public interface ISourceWriter
    {
        /// <summary>
        /// Writes a blank line into the code being generated.
        /// </summary>
        void BlankLine();

        /// <summary>
        /// Writes a line with an opening '{' character at the current block level
        /// and increments the current block level.
        /// </summary>
        /// <param name="text">The text to write (try/while/do/foreach etc.).</param>
        void Block(string text);

        /// <summary>
        /// Writes one or more lines into the code that respects the current block depth
        /// and handles text alignment for you.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void WriteLines(string text = null);

        /// <summary>
        /// Writes a single line with this content to the code
        /// at the current block level.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void WriteLine(string text);

        /// <summary>
        /// Writes a line with a closing '}' character at the current block level
        /// and decrements the current block level.
        /// </summary>
        /// <param name="extra">The (optional) text to write.</param>
        void FinishBlock(string extra = null);

        /// <summary>
        /// Gets the current indentation level of this writer (i.e. increased within an if/try/while/* scope, decreased
        /// outside).
        /// </summary>
        /// <see cref="Block" />
        /// <see cref="FinishBlock" />
        int IndentationLevel { get; }
    }
}
