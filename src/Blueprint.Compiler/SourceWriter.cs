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
        private static readonly string[] _indentationLevels =
        {
            string.Empty,
            new string(' ', 1 * 4),
            new string(' ', 2 * 4),
            new string(' ', 3 * 4),
            new string(' ', 4 * 4),
            new string(' ', 5 * 4),
        };

        private readonly StringBuilder _writer;

        private int _level;
        private string _leadingSpaces = string.Empty;

        /// <summary>
        /// Initialises a new instance of the <see cref="SourceWriter" /> class.
        /// </summary>
        /// <param name="capacity">The suggested initial capacity of the underlying <see cref="StringBuilder" />.</param>
        public SourceWriter(int capacity = 2048)
        {
            this._writer = new StringBuilder(capacity);
        }

        /// <inheritdoc />
        public int IndentationLevel
        {
            get => this._level;

            set
            {
                this._level = value;
                this._leadingSpaces = this._level < _indentationLevels.Length ? _indentationLevels[this._level] : new string(' ', this._level * 4);
            }
        }

        /// <inheritdoc />
        public ISourceWriter BlankLine()
        {
            this._writer.Append("\r\n");

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Indent()
        {
            this._writer.Append(this._leadingSpaces);

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Append(string text)
        {
            this._writer.Append(text);

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Append(char c)
        {
            this._writer.Append(c);

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter WriteLines(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                this.BlankLine();

                return this;
            }

            foreach (ReadOnlySpan<char> line in text.SplitLines())
            {
                if (line.IsWhiteSpace())
                {
                    this.BlankLine();
                }
                else
                {
                    this._writer.Append(this._leadingSpaces);
                    this._writer.Append(line.ToString());
                    this._writer.AppendLine();
                }
            }

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter WriteLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                this._writer.AppendLine();
            }
            else
            {
                this._writer.Append(this._leadingSpaces);
                this._writer.Append(text);
                this._writer.AppendLine();
            }

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter Block(string text)
        {
            this.WriteLine(text);
            this.WriteLine("{");

            this.IndentationLevel++;

            return this;
        }

        /// <inheritdoc />
        public ISourceWriter FinishBlock(string extra = null)
        {
            if (this.IndentationLevel == 0)
            {
                throw new InvalidOperationException("Not currently in a code block");
            }

            this.IndentationLevel--;

            if (string.IsNullOrEmpty(extra))
            {
                this.WriteLine("}");
            }
            else
            {
                this.WriteLine("}" + extra);
            }

            return this;
        }

        /// <summary>
        /// Returns the code that has been written to this <see cref="SourceWriter" />.
        /// </summary>
        /// <returns></returns>
        public string Code()
        {
            return this._writer.ToString();
        }
    }
}
