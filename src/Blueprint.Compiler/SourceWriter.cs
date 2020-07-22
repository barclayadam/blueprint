using System;
using System.IO;

using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    public class SourceWriter : ISourceWriter
    {
        private readonly StringWriter writer = new StringWriter();

        private int level;
        private string leadingSpaces = string.Empty;

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

        public void BlankLine()
        {
            writer.WriteLine();
        }

        public void Write(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                BlankLine();
                return;
            }

            text.ReadLines(line =>
            {
                if (string.IsNullOrEmpty(line))
                {
                    BlankLine();
                }
                else
                {
                    WriteLine(line);
                }
            });
        }

        public void WriteLine(string text)
        {
            writer.WriteLine(leadingSpaces + text);
        }

        public void Block(string text)
        {
            WriteLine(text);
            WriteLine("{");

            IndentationLevel++;
        }

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

        public string Code()
        {
            return writer.ToString();
        }
    }
}
