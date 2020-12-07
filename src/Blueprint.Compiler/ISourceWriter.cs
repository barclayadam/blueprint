namespace Blueprint.Compiler
{
    public interface ISourceWriter
    {
        /// <summary>
        /// Writes a blank line into the code being generated.
        /// </summary>
        /// <returns>This source writer.</returns>
        ISourceWriter BlankLine();

        /// <summary>
        /// Appends the current indentation level to this writer, which should only be used
        /// in the case of directly writing to this writer through the <see cref="Append(string)"/> or
        /// <see cref="Append(char)"/> methods, as <see cref="WriteLine"/> and <see cref="WriteLines"/> will
        /// already handle automatic indentation.
        /// </summary>
        /// <returns>This source writer.</returns>
        ISourceWriter Indent();

        /// <summary>
        /// Writes a line with an opening '{' character at the current block level
        /// and increments the current block level.
        /// </summary>
        /// <param name="text">The text to write (try/while/do/foreach etc.).</param>
        /// <returns>This source writer.</returns>
        ISourceWriter Block(string text);

        /// <summary>
        /// Appends the given text to this source writer but DOES NOT append a new line
        /// character.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <returns>This source writer.</returns>
        ISourceWriter Append(string text);

        /// <summary>
        /// Appends the given text to this source writer but DOES NOT append a new line
        /// character.
        /// </summary>
        /// <param name="c">The character to append.</param>
        /// <returns>This source writer.</returns>
        ISourceWriter Append(char c);

        /// <summary>
        /// Writes one or more lines into the code that respects the current block depth
        /// and handles text alignment for you.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <returns>This source writer.</returns>
        ISourceWriter WriteLines(string text = null);

        /// <summary>
        /// Writes a single line with this content to the code
        /// at the current block level.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <returns>This source writer.</returns>
        ISourceWriter WriteLine(string text);

        /// <summary>
        /// Writes a line with a closing '}' character at the current block level
        /// and decrements the current block level.
        /// </summary>
        /// <param name="extra">The (optional) text to write.</param>
        /// <returns>This source writer.</returns>
        ISourceWriter FinishBlock(string extra = null);

        /// <summary>
        /// Gets the current indentation level of this writer (i.e. increased within an if/try/while/* scope, decreased
        /// outside).
        /// </summary>
        /// <see cref="Block" />
        /// <see cref="FinishBlock" />
        int IndentationLevel { get; }
    }
}
