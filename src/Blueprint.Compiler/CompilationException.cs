using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    /// <summary>
    /// Throws when the compilation of an assembly has failed.
    /// </summary>
    public class CompilationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="failures">All failures that were found during compilation.</param>
        /// <param name="sourceCode">The source code that was attempted to be compiled.</param>
        public CompilationException(string message, IEnumerable<Diagnostic> failures, string sourceCode)
            : base(message)
        {
            this.Failures = failures;
            this.SourceCode = sourceCode;
        }

        /// <summary>
        /// All failures that were found during compilation.
        /// </summary>
        public IEnumerable<Diagnostic> Failures { get; }

        /// <summary>
        /// The source code that was attempted to be compiled.
        /// </summary>
        public string SourceCode { get; }
    }
}
