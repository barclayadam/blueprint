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

        public int IndentionLevel
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
                line = line.Replace('`', '"');

                if (string.IsNullOrEmpty(line))
                {
                    BlankLine();
                }
                else if (line.StartsWith("BLOCK:"))
                {
                    WriteLine(line.Substring(6));
                    StartBlock();
                }
                else if (line.StartsWith("END"))
                {
                    FinishBlock(line.Substring(3));
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

        public void FinishBlock(string extra = null)
        {
            if (IndentionLevel == 0)
            {
                throw new InvalidOperationException("Not currently in a code block");
            }

            IndentionLevel--;

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

        private void StartBlock()
        {
            WriteLine("{");
            IndentionLevel++;
        }
    }
}
