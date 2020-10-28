using System;
using System.Text;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    /// <summary>
    /// Default implementation of <see cref="ISourceWriter" /> that uses a backing <see cref="StringBuilder" />.
    /// </summary>
    public class SourceWriter : ISourceWriter
    {
        private static readonly string[] indentationLevels =
        {
            string.Empty,
            new string(' ', 1 * 4),
            new string(' ', 2 * 4),
            new string(' ', 3 * 4),
            new string(' ', 4 * 4),
            new string(' ', 5 * 4),
        };

        private readonly StringBuilder writer;

        private int level;
        private string leadingSpaces = string.Empty;

        /// <summary>
        /// Initialises a new instance of the <see cref="SourceWriter" /> class.
        /// </summary>
        /// <param name="capacity">The suggested initial capacity of the underlying <see cref="StringBuilder" />.</param>
        public SourceWriter(int capacity = 2048)
        {
            writer = new StringBuilder(capacity);
        }

        /// <inheritdoc />
        public int IndentationLevel
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
                leadingSpaces = level < indentationLevels.Length ? indentationLevels[level] : new string(' ', level * 4);
            }
        }

        /// <inheritdoc />
        public ISourceWriter BlankLine()
        {
            writer.Append("\r\n");

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Append(string text)
        {
            writer.Append(text);

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Append(char c)
        {
            writer.Append(c);

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter WriteLines(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                BlankLine();

                return this;
            }

            foreach (ReadOnlySpan<char> line in text.SplitLines())
            {
                if (line.IsWhiteSpace())
                {
                    BlankLine();
                }
                else
                {
                    writer.Append(leadingSpaces);
                    writer.Append(line.ToString());
                    writer.AppendLine();
                }
            }

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter WriteLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                writer.AppendLine();
            }
            else
            {
                writer.Append(leadingSpaces);
                writer.Append(text);
                writer.AppendLine();
            }

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Block(string text)
        {
            WriteLine(text);
            WriteLine("{");

            IndentationLevel++;

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter FinishBlock(string extra = null)
        {
            if (IndentationLevel == 0)
            {
                throw new InvalidOperationException("Not currently in a code block");
            }

            IndentationLevel--;

            if (string.IsNullOrEmpty(extra))
            {
                WriteLine("}");
            }
            else
            {
                WriteLine("}" + extra);
            }

            BlankLine();

            return this;
        }

        /// <inheritdoc />
        public string Code()
        {
            return writer.ToString();
        }
    }
}
