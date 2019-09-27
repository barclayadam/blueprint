using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {
        }

        public string Code { get; internal set; }

        public IEnumerable<Diagnostic> Failures { get; internal set; }
    }
}
