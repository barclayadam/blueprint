using System;

namespace Blueprint.Compiler
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {
        }

        public string Code { get; set; }
    }
}
