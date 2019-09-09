using System.IO;
using System.Linq;

using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    internal class SourceCodeParser
    {
        private readonly LightweightCache<string, string> code = new LightweightCache<string, string>(name => "UNKNOWN");

        private readonly StringWriter current;
        private readonly string name;

        internal SourceCodeParser(string code)
        {
            foreach (var line in code.ReadLines())
            {
                if (current == null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.Trim().StartsWith("// START"))
                    {
                        name = line.Split(':').Last().Trim();
                        current = new StringWriter();
                    }
                }
                else
                {
                    if (line.Trim().StartsWith("// END"))
                    {
                        var classCode = current.ToString();
                        this.code[name] = classCode;

                        current = null;
                        name = null;
                    }
                    else
                    {
                        current.WriteLine(line);
                    }
                }

            }
        }

        public string CodeFor(string typeName)
        {
            return code[typeName];
        }
    }
}
