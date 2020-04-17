using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that simply writes a blank line to the source, useful for formatting purposes should
    /// the generated code need to be shown for diagnostics.
    /// </summary>
    public class BlankLineFrame : SyncFrame
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.BlankLine();

            next();
        }
    }
}
