namespace Blueprint.Compiler.Frames
{
    public class CommentFrame : SyncFrame
    {
        private readonly string commentText;

        public CommentFrame(string commentText)
        {
            this.commentText = commentText;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteComment(commentText);

            Next?.GenerateCode(method, writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"// {commentText}";
        }
    }
}
