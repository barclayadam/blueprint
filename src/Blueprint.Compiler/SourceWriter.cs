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
        private readonly StringBuilder writer = new StringBuilder();

        private int level;
        private string leadingSpaces = string.Empty;

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
                leadingSpaces = new string(' ', level * 4);
            }
        }

        /// <inheritdoc />
        public void BlankLine()
        {
            writer.Append("\r\n");
        }

        /// <inheritdoc />
        public void Write(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                BlankLine();
                return;
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
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            writer.Append(leadingSpaces);
            writer.Append(text);
            writer.AppendLine();
        }

        /// <inheritdoc />
        public void Block(string text)
        {
            WriteLine(text);
            WriteLine("{");

            IndentationLevel++;
        }

        /// <inheritdoc />
        public void FinishBlock(string extra = null)
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
        }

        /// <inheritdoc />
        public string Code()
        {
            return writer.ToString();
        }
    }
}
