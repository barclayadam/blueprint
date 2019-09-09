using System;
using System.IO;

using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    public class SourceWriter : ISourceWriter
    {
        private readonly StringWriter writer = new StringWriter();
        private string leadingSpaces = "";

        private int level;

        public int IndentionLevel
        {
            get { return level; }
            set
            {
                level = value;
                leadingSpaces = "".PadRight(level*4);
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

        private void StartBlock()
        {
            WriteLine("{");
            IndentionLevel++;
        }

        public void FinishBlock(string extra = null)
        {
            if (IndentionLevel == 0)
            {
                throw new InvalidOperationException("Not currently in a code block");
            }

            IndentionLevel--;

            if (string.IsNullOrEmpty(extra))
                WriteLine("}");
            else
                WriteLine("}" + extra);


            BlankLine();
        }

        public string Code()
        {
            return writer.ToString();
        }

        internal class BlockMarker : IDisposable
        {
            private readonly SourceWriter parent;

            public BlockMarker(SourceWriter parent)
            {
                this.parent = parent;
            }

            public void Dispose()
            {
                parent.FinishBlock();
            }
        }
    }
}
