namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that simply writes a blank line to the source, useful for formatting purposes should
    /// the generated code need to be shown for diagnostics.
    /// </summary>
    public class BlankLineFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "[blank]";
        }
    }
}
